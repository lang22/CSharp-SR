using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;


using System.Threading;



namespace SlidingWindow
{
    enum Move { SEND = 1, RECEIVE, SENDACK, RCVACK, HANDIN, S_SLIDE, R_SLIDE, LOST, ACKLOST };
    struct Result
    {
        public Move move;
        public int seq;
        public long time_start;
    }
    struct Packet
    {
        public bool isSent;
        public bool isReceived;
        public bool isSentACK;
        public bool isReceiveACK;
        public int TTR;
    }
    class Simulator
    {
        private int N = 4;
        private const int NUM_PACKET = 14;
        private int loss_rate;
        private int send_base;
        private int rcv_base;
        private int move_count;
        private int handin_count;//最近一个未被提交的包序号
        private int finish_count;
        private Object lk;
        private Packet[] packets;
        public Result[] results;
        private Random rd = new Random();

        public Simulator()
        {

            loss_rate = 0;
            results = new Result[2000];
            packets = new Packet[NUM_PACKET + 100];
            send_base = 0;
            rcv_base = 0;
            move_count = 0;
            handin_count = 0;
            finish_count = 0;
            Init(packets);
            lk = new object();//线程锁
        }
        public Simulator(int loss_rate, int window_sise)
        {
            this.loss_rate = loss_rate;
            N = window_sise;
            results = new Result[1000];
            packets = new Packet[NUM_PACKET + 100];
            send_base = 0;
            rcv_base = 0;
            move_count = 0;
            handin_count = 0;
            finish_count = 0;
            Init(packets);
            lk = new object();//线程锁
        }

        public Result[] Simulate(ref bool isFinish)
        {
            long startTime = (long)GetTimestamp(DateTime.Now);//记下开始的时间戳

            //用来模拟发送方的发送线程
            new Thread(new ThreadStart(() =>
            {
                int lasttime_base, period = 0;
                //模拟效果：每隔一段时间判断窗口内是否有：
                //①没有发送的②发送了还没收到ACK的且定时器到时的
                //有则立刻发送（改变isSent，往results里写入一个动作，所有TTR-1）然后sleep随机时间
                //窗口内所有ACK都收到了并且窗口尾到底了则终止进程
                while (finish_count < NUM_PACKET)
                {
                    int current_send_base = send_base;
                    lasttime_base = current_send_base;
                    if (lasttime_base != current_send_base) { period = 0; lasttime_base = current_send_base; }
                    if (period >= N) period = 0;
                    for (int i = 0 + period; i < N; i++)
                    {
                        if (packets[current_send_base + i].isSent && !packets[current_send_base + i].isReceiveACK && packets[current_send_base + i].TTR <= 0)//有还未发送的
                        {
                            int temp = current_send_base + i;
                            bool isSent = false;
                            while (true)
                            {
                                lock (lk)
                                {
                                    if (RandomSend())
                                    {
                                        AddMove(Move.SEND, temp, (long)GetTimestamp(DateTime.Now) - startTime);
                                        isSent = true;
                                    }
                                    else
                                    {
                                        AddMove(Move.LOST, temp, (long)GetTimestamp(DateTime.Now) - startTime);

                                        isSent = false;
                                    }
                                    break;
                                }
                            }
                            if (isSent)
                            {
                                new Thread(new ThreadStart(() =>
                                {
                                    Thread.Sleep(300);//模拟传送时间
                                    if (temp >= 0 && temp <= 13)
                                    {
                                        while (true)
                                        {
                                            lock (lk)
                                            {
                                                packets[temp].isReceived = true;//接受方才接受
                                                packets[temp].isSentACK = false;
                                                AddMove(Move.RECEIVE, temp, (long)GetTimestamp(DateTime.Now) - startTime);
                                                break;
                                            }
                                        }
                                    }
                                })).Start();
                            }

                            packets[temp].TTR = 6;//重发的TTR重新计算
                            break;
                        }
                        else if (!packets[current_send_base + i].isSent)//有发送了且定时器超时了的
                        {
                            int temp = current_send_base + i;
                            bool isSent = false;
                            while (true)
                            {
                                lock (lk)
                                {
                                    if (RandomSend() || temp >= 10 || temp == 0 || temp == 1)
                                    {
                                        AddMove(Move.SEND, temp, (long)GetTimestamp(DateTime.Now) - startTime);
                                        isSent = true;
                                    }
                                    else
                                    {
                                        AddMove(Move.LOST, temp, (long)GetTimestamp(DateTime.Now) - startTime);
                                        isSent = false;
                                    }

                                    break;
                                }
                            }
                            if (isSent)
                            {
                                new Thread(new ThreadStart(() =>
                                {
                                    Thread.Sleep(300);//模拟传送时间
                                    if (temp >= rcv_base && temp < rcv_base + N)//在窗口内
                                    {
                                        while (true)
                                        {
                                            lock (lk)
                                            {
                                                packets[temp].isReceived = true;//接受方才接受
                                                AddMove(Move.RECEIVE, temp, (long)GetTimestamp(DateTime.Now) - startTime);
                                                break;
                                            }
                                        }

                                    }
                                })).Start();

                            }
                            packets[temp].isSent = true;
                            packets[temp].TTR = 6;//重发的TTR重新计算
                            break;
                        }
                    }
                    MinusTTR();//每过一个发送时段，窗口内TTR减1
                    period++;
                    Thread.Sleep(rd.Next(100, 130));
                }
            })).Start();

            //用来模拟发送方的接受ACK线程
            new Thread(new System.Threading.ThreadStart(() =>
            {
                //模拟效果：持续判断窗口内有无ACK回来了但是还没进行滑动操作的，有则立刻进行
                while (finish_count < NUM_PACKET)
                {
                    if (packets[send_base].isReceiveACK)
                    {
                        while (true)
                        {
                            lock (lk)
                            {
                                finish_count++;
                                send_base++;
                                AddMove(Move.S_SLIDE, -1, (long)GetTimestamp(DateTime.Now) - startTime);
                                break;
                            }
                        }
                    }
                    Thread.Sleep(2);

                }
            })).Start();


            //用来模拟接收方的接收线程
            new Thread(new System.Threading.ThreadStart(() =>
            {
                //模拟效果：持续判断窗口基位置是否已经抵达并发送ACK，是则提交并边滑动边提交
                while (finish_count < NUM_PACKET)
                {
                    if (packets[rcv_base].isReceived && packets[rcv_base].isSentACK)
                    {

                        while (true)
                        {
                            lock (lk)
                            {
                                AddMove(Move.HANDIN, rcv_base, (long)GetTimestamp(DateTime.Now) - startTime);
                                AddMove(Move.R_SLIDE, -1, (long)GetTimestamp(DateTime.Now) - startTime);
                                break;
                            }
                        }
                        rcv_base++;
                    }
                    Thread.Sleep(2);
                }
            })).Start();


            //用来模拟接收方的发送ACK进程
            new Thread(new System.Threading.ThreadStart(() =>
            {
                //模拟效果：持续检查是否有窗口内以及窗口前一段窗口内有无抵达了但是还未回发ACK的，有则立即发
                while (finish_count < NUM_PACKET)
                {
                    int current_rcv_base = rcv_base;
                    if (current_rcv_base < N)
                    {
                        for (int i = 0; i < current_rcv_base + N; i++)
                        {

                            if (packets[i].isReceived && !packets[i].isSentACK)//抵达了但是还未回发ACK
                            {
                                packets[i].isSentACK = true;

                                int temp = i;
                                if (RandomSend())
                                {
                                    while (true) { lock (lk) { AddMove(Move.SENDACK, temp, (long)GetTimestamp(DateTime.Now) - startTime); break; } }
                                    new Thread(new ThreadStart(() =>
                                    {

                                        Thread.Sleep(300);//模拟传送时间         
                                        while (true) { lock (lk) { AddMove(Move.RCVACK, temp, (long)GetTimestamp(DateTime.Now) - startTime); break; } }

                                        packets[temp].isReceiveACK = true;//发送方接受了ACK
                                    })).Start();
                                }
                                else while (true) { lock (lk) { AddMove(Move.ACKLOST, temp, (long)GetTimestamp(DateTime.Now) - startTime); break; } }




                            }
                            Thread.Sleep(2);
                        }
                    }
                    else
                    {
                        for (int i = -N; i < N; i++)
                        {
                            if (packets[current_rcv_base + i].isReceived && !packets[current_rcv_base + i].isSentACK)//抵达了但是还未回发ACK
                            {
                                packets[current_rcv_base + i].isSentACK = true;
                                int temp = i;
                                if (RandomSend())
                                {
                                    while (true) { lock (lk) { AddMove(Move.SENDACK, current_rcv_base + i, (long)GetTimestamp(DateTime.Now) - startTime); break; } }

                                    new Thread(new ThreadStart(() =>
                                    {
                                        Thread.Sleep(300);//模拟传送时间
                                        while (true) { lock (lk) { AddMove(Move.RCVACK, current_rcv_base + temp, (long)GetTimestamp(DateTime.Now) - startTime); break; } }

                                        packets[current_rcv_base + temp].isReceiveACK = true;//发送方接受了ACK
                                    })).Start();

                                }

                                else while (true) { lock (lk) { AddMove(Move.ACKLOST, current_rcv_base + i, (long)GetTimestamp(DateTime.Now) - startTime); break; } }

                            }
                        }
                        Thread.Sleep(2);
                    }
                }
            })).Start();

            while (finish_count < NUM_PACKET) { }//卡住线程，防止提前返回
            isFinish = true;
            return results;
        }




        private void Init(Packet[] packets)
        {
            for (int i = 0; i < NUM_PACKET + 100; i++)
            {
                packets[i].isSent = packets[i].isReceived =
                    packets[i].isSentACK = packets[i].isReceiveACK = false;
                packets[i].TTR = 6;
            }
        }
        public void AddMove(Move move, int seq, long time_line)
        {


            results[move_count].move = move;
            results[move_count].seq = seq;
            results[move_count++].time_start = time_line;
            /*
            Console.Write(move);
            Console.Write("\t\t\t\t");
            Console.Write(seq);
            Console.Write("\t");
            Console.Write(time_line);
            Console.Write("\t\t");
            Console.Write(send_base);
            Console.Write("\t");
            Console.Write(rcv_base);
            Console.Write("\t");
            Console.Write(finish_count);
            Console.Write("\n");
            */

        }
        public void MinusTTR()//将窗口内已发送但是还未接收到ACK的TTR减一
        {
            for (int i = 0; i < N; i++) if (packets[send_base + i].isSent && !packets[send_base + i].isReceiveACK) packets[send_base + i].TTR--;
        }
        public static double GetTimestamp(DateTime d)
        {
            TimeSpan ts = d - new DateTime(1970, 1, 1);
            return ts.TotalMilliseconds;
        }

        public bool RandomSend()
        {

            return rd.Next(1, 100) > loss_rate;
        }

    }
}
