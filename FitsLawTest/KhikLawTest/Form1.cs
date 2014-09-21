using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestLib;

namespace KhikLawTest
{
    public partial class Form1 : Form
    {
        Button[] buttons;
        KhikDecisionTest currentTest;
        Random rand = new Random();
        Button correctButton;
        int[] labelValues = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public Form1()
        {
            InitializeComponent();
            buttons = new Button[]{test0,test1,test2,test3,test4,test5,test6,test7,test8};
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            if (e.Button == MouseButtons.Right && !currentTest.IsTrying)
            {
                SetupMouseAndButtons();
                currentTest.StartTry();
            }
        }

        private void SetupMouseAndButtons()
        {
            Cursor.Position = this.PointToScreen(new Point(200,150));

            if(correctButton != null)
            {
                correctButton.MouseClick -= OnCorrectButtonClick;
            }

            int buttonsCount = currentTest.TestValue;            
           
            var values = labelValues.OrderBy(val => rand.Next()).Take(buttonsCount).ToArray();

            int correctButtonIndex = rand.Next(buttonsCount);
            int correctValue = values[correctButtonIndex];
            correctValueLabel.Text = correctValue.ToString();

            for (int i = 0; i < 9; i++)
            {
                if (i < buttonsCount)
                {
                    buttons[i].Show();
                    buttons[i].Text = values[i].ToString();
                }
                else                
                    buttons[i].Hide();
            }

            
            correctButton = buttons[correctButtonIndex];
            correctButton.MouseClick += OnCorrectButtonClick;
        }


        private void OnCorrectButtonClick(object sender, EventArgs e)
        {
            currentTest.FinishTry(true);
        }



        private void startButton_Click(object sender, EventArgs e)
        {
            StartTest(new KhikDecisionTest(new int[] { 2, 3, 4, 5, 6, 7, 8, 9 }));
        }
 
        public void StartTest(KhikDecisionTest test)
        {
            startButton.Hide();
            
            if (currentTest != null)
                currentTest.OnTestCompleted -= OnTestCompleted;
            
            currentTest = test;
            currentTest.OnTestCompleted += OnTestCompleted;
        }

        private void OnTestCompleted(List<TryResult<TimeSpan>> testResults)
        {
            Utils.SaveResults<TimeSpan>("Результаты теста.txt", testResults, s => s.ToString(@"s\.fff"));
            MessageBox.Show("Тест завершен. Результаты сохранены");
            startButton.Show();
        }




    }
}
