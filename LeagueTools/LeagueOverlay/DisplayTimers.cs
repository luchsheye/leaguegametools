using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace LeagueOverlay
{
    public class DisplayTimers
    {
        //some option variables
        static FontFamily timerFont = new FontFamily("Comic Sans");

        public class Timer
        {
            public Timer(string name, TimeSpan timeLeft) { this.name = name; this.timeLeft = timeLeft; }
            public string name;
            public TimeSpan timeLeft;
        }

        static DateTime lastUpdateTime = DateTime.Now;

        public static List<Timer> timers = new List<Timer>();

        public static void createTimer(string name, TimeSpan durration)
        {
            timers.Add(new Timer(name, durration));
        }

        public static void updateTimers()
        {
            TimeSpan elapsed = DateTime.Now - lastUpdateTime;
            foreach (Timer t in timers)
                t.timeLeft -= elapsed;
            timers.RemoveAll(t => t.timeLeft.TotalSeconds < 0);
            lastUpdateTime = DateTime.Now;
        }

        public static int previousDrawHeight = 0;
        public static int previousDrawWidth = 0;

    }
}
