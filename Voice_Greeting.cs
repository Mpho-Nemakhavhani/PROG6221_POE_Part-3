using System;
using System.Collections.Generic;
using System.Media;
using System.Text;
using System.Windows;

namespace CyberSecurityChatBotPart2
{
    class Voice_Greeting
    {
        public static void PlayGreeting() 
        {
            try
            {
                SoundPlayer player = new SoundPlayer("Greeting.wav.wav");
                player.Play();
            }
            catch 
            {
                MessageBox.Show("Audio could not play");
            }

        }
    }
}
