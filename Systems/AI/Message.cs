using System.Collections.Generic;
using UnityEngine;


namespace Message
{
    public struct Telegram
    {
        Entity sender;
        Entity reciever;
        int msg;
        float dispatchTime;
        static double smallestDelay;

        public Entity Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        public Entity Reciever
        {
            get { return reciever; }
            set { sender = value; }
        }

        public float DispatchTime
        {
            get { return dispatchTime; }
            set { dispatchTime = value; }
        }

        public Telegram(Entity _sender, Entity _reciever, int _msg, float _time, double _smallestDelay = 0.25f)
        {
            sender = _sender;
            reciever = _reciever;
            msg = _msg;
            dispatchTime = _time;
            smallestDelay = _smallestDelay;
        }


        public static bool operator == (Telegram t1, Telegram t2)
        {
            return ((Mathf.Abs(t1.dispatchTime - t2.dispatchTime)) > smallestDelay) &&
                (t1.sender == t2.sender) &&
                (t1.reciever == t2.reciever) &&
                (t1.msg == t2.msg);
        }

        public static bool operator != (Telegram t1, Telegram t2)
        {
            return ((Mathf.Abs(t1.dispatchTime - t2.dispatchTime)) > smallestDelay) &&
                (t1.sender == t2.sender) &&
                (t1.reciever == t2.reciever) &&
                (t1.msg == t2.msg);
        }

        public static bool operator < (Telegram t1, Telegram t2)
        {
            if (t1 == t2)
            {
                return false;
            }
            else
            {
                return (t1.dispatchTime < t2.dispatchTime);
            }
        }

        public static bool operator > (Telegram t1, Telegram t2)
        {
            if (t1 == t2)
            {
                return false;
            }
            else
            {
                return (t1.dispatchTime > t2.dispatchTime);
            }
        }

        public override bool Equals(object obj)
        {
            if((obj == null) || !(obj is Telegram))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode() { return 1; }

    }

    public class MessageDispatcher
    {
        public static int SEND_MSG_IMMEDIATELY = -1;
        public static int NO_ADDITIONAL_INFO = -1;
        public static int SENDER_ID_IRREVELANT = -1;

        static MessageDispatcher instance;

        private HashSet<Telegram> priorityQueue = new HashSet<Telegram>();
        float currentTime = 0.0f;

        MessageDispatcher() { }

        public static MessageDispatcher Instance()
        {
            if(instance == null)
            {
                instance = new MessageDispatcher();
            }
            return instance;
        }

        void Discharge(Entity reciever, Telegram telegram)
        {
            if (!reciever.HandleMessage(telegram))
            {
                Debug.Log("message could not be handled");
            }
        }

        public void DispatchMessage(float delay, Entity sender, Entity reciever, int msg)
        {
            Telegram telegram = new Telegram(sender, reciever, msg, 0);

            if(delay < 0.0)
            {
                Discharge(reciever, telegram);
            }
            else
            {
                float currentTime = Time.time;
                telegram.DispatchTime = currentTime + delay;
                priorityQueue.Add(telegram);
            }
        }

        public void DispatchDelayedMessages()
        {
            currentTime += Time.time;

            foreach (Telegram telegram in priorityQueue )
            {
                if (currentTime >= telegram.DispatchTime && telegram.DispatchTime > 0)
                {
                    Entity reciever = telegram.Reciever;
                    Discharge(reciever, telegram);
                    priorityQueue.Remove(telegram);
                }
            }
            // currentTime = 0;
        }
    }
}