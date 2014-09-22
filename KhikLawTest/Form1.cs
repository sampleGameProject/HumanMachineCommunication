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

        Color defaultColor;
        Font defaultFont;
        
        Color[] testColors;
        Font[] testFonts;
        MutableInt testColorCounter;

        bool complexTest;        

        public Form1()
        {
            InitializeComponent();
            buttons = new Button[]{test0,test1,test2,test3,test4,test5,test6,test7,test8};

            testColors = new Color[] { Color.White, Color.Black, Color.DimGray, Color.Green, Color.Red };
            testFonts = new Font[] 
            { 
                new Font(new FontFamily("Arial"),   11, FontStyle.Underline), 
                new Font(new FontFamily("Courier"), 10, FontStyle.Underline), 
                new Font(new FontFamily("Times"),   12, FontStyle.Underline)
            };

            defaultColor = test0.ForeColor;
            defaultFont = test0.Font;
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
            correctButton = buttons[correctButtonIndex];
            correctButton.MouseClick += OnCorrectButtonClick;


            if (!complexTest)
            {
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
            }
            else
            {
                correctValueLabel.Hide();

                for (int i = 0; i < 9; i++)
                {
                    if (i < buttonsCount)
                    {
                        buttons[i].Show();
                        buttons[i].Text = "Click me";
                    }
                    else
                        buttons[i].Hide();

                    buttons[i].Font = defaultFont;
                    buttons[i].ForeColor = defaultColor;
                }

                correctButton.ForeColor = testColors[testColorCounter.Value];
                correctButton.Font = testFonts[rand.Next(testFonts.GetLength(0))];
            }

            
            
        }


        private void OnCorrectButtonClick(object sender, EventArgs e)
        {
            currentTest.FinishTry(true);
        }



        private void startButton_Click(object sender, EventArgs e)
        {
            StartTest(new KhikDecisionTest(new int[] { 2, 3, 4, 5, 6, 7, 8, 9 }));
        }


        void OnColorTestIterationChanged(int newValue)
        {
            StartTest(new KhikDecisionTest(new int[] { 2, 3, 4, 5, 6, 7, 8, 9 }));
        }
 

        void OnColorTestCompleted()
        {
            MessageBox.Show("Комплексный тест завершен");
            complexTest = false;
        }

        private void StartComplexTestClick(object sender, EventArgs e)
        {
            complexTest = true;
            testColorCounter = new MutableInt(new int[] { 0, 1, 2, 3, 4 });
            testColorCounter.OnValueChanged += OnColorTestIterationChanged;
            testColorCounter.OnMutationComplete += OnColorTestCompleted;

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
            Utils.SaveResults<TimeSpan>(GetTestResultsFileName(), testResults, s => s.ToString(@"s\.fff"));

            if(!complexTest)
            {
                MessageBox.Show("Тест завершен. Результаты сохранены");
                startButton.Show();
            }
            else
            {
                testColorCounter.Mutate();
            }
           
        }

        private string GetTestResultsFileName()
        {
            if(complexTest)
            {
                return string.Format("Результаты комплексного теста. Итерация № {0}.txt", testColorCounter.Value + 1);
            }
            else
            {
                return "Результаты простого теста.txt";
            }
        }



    }
}
