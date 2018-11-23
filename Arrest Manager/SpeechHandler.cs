using Rage;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arrest_Manager
{
   internal class SpeechHandler
    {
        public static bool DisplayTime = false;
        private static List<string> Answers;
        //public enum AnswersResults { Positive, Negative, Neutral, Null};
        public static int DisplayAnswers(List<string> PossibleAnswers)
        {
            Game.FrameRender += DrawAnswerWindow;
            DisplayTime = true;
            Answers = PossibleAnswers;
            string AnswerGiven = "";
            Game.LocalPlayer.Character.IsPositionFrozen = true;
            
            GameFiber.StartNew(delegate
            {
                while (DisplayTime)
                {
                    GameFiber.Yield();
                    
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.D1))
                    {
                        if (Answers.Count >= 1)
                        {
                            AnswerGiven = Answers[0];

                        }
                    }
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.D2))
                    {
                        if (Answers.Count >= 2)
                        {
                            AnswerGiven = Answers[1];
                        }
                    }
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.D3))
                    {
                        if (Answers.Count >= 3)
                        {
                            AnswerGiven = Answers[2];
                        }
                    }
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.D4))
                    {
                        if (Answers.Count >= 4)
                        {
                            AnswerGiven = Answers[3];
                        }
                    }
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.D5))
                    {
                        if (Answers.Count >= 5)
                        {
                            AnswerGiven = Answers[4];
                        }
                    }
                }
            });
            while (AnswerGiven == "")
            {
                GameFiber.Yield();
                if (!DisplayTime) { break; }
            }

            DisplayTime = false;
            Game.LocalPlayer.Character.IsPositionFrozen = false;

            return PossibleAnswers.IndexOf(AnswerGiven);


        }

        private static void DrawAnswerWindow(System.Object sender, Rage.GraphicsEventArgs e)
        {
            if (DisplayTime)
            {
                Rectangle drawRect = new Rectangle(Game.Resolution.Width / 4, Game.Resolution.Height / 7, 660, 170);
                Rectangle drawBorder = new Rectangle(Game.Resolution.Width / 4 - 5, Game.Resolution.Height / 7 - 5, 660, 170);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                e.Graphics.DrawRectangle(drawBorder, Color.FromArgb(90, Color.Black));
                e.Graphics.DrawRectangle(drawRect, Color.Black);

                e.Graphics.DrawText("Select with Number Keys", "Aharoni Bold", 18.0f, new PointF(drawBorder.X + 150, drawBorder.Y + 2), Color.White, drawBorder);
                int YIncreaser = 30;
                for (int i = 0; i < Answers.Count; i++)
                {

                    e.Graphics.DrawText("[" + (i + 1).ToString() + "] " + Answers[i], "Arial Bold", 15.0f, new PointF(drawRect.X + 10, drawRect.Y + YIncreaser), Color.White, drawRect);
                    YIncreaser += 25;
                }


            }
            else
            {
                Game.FrameRender -= DrawAnswerWindow;
            }


        }
    }
}
