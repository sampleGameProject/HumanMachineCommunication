using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestLib
{

    public class D
    {
        public delegate void ValueChangedDelegate(int newValue);
        public delegate void VoidDelegate();
        public delegate void CompletedDelegate<T>(List<TryResult<T>> testResults);
    }

    public class MutableInt
    {
        public event D.ValueChangedDelegate OnValueChanged;
        public event D.VoidDelegate OnMutationComplete;

        private int[] _values;
        private int _index;
        private int _repeatCount;
        private int _repeat;

        public MutableInt(int[] values, int repeatCount)
        {
            _values = values;
            _index = 0;
            _repeatCount = repeatCount;
            _repeat = 0;
        }

        public MutableInt(int[] values) : this(values,1) { }

        public void Mutate()
        {
            _repeat++;
            if (_repeat == _repeatCount)
            {
                _index++;
                if (_index >= ValuesCount)
                {
                    if (OnMutationComplete != null)
                         OnMutationComplete();                
                }
                else
                {
                    if (OnValueChanged != null)
                        OnValueChanged(Value);
                    _repeat = 0;
                }
            }
        }

        public int Value { get { return _values[_index]; } }
        public int ValuesCount { get { return _values.GetLength(0); } }
    }

    public class TryResult<T>
    {
        public int Value { get; set; }
        public T TValue { get; set; }

        public TryResult() : 
            this(0)
        {
          
        }

        public TryResult(int tryIndex)
        {
            Value = tryIndex;
            TValue = default(T);
        }
    }

    public abstract class BaseTest<T>
    {
        protected TimeSpan _tryTimeLimit = new TimeSpan(0, 0, 3);
        protected DateTime? _tryStart;
        protected TimeSpan _lastTryTime;

        protected List<TryResult<T>> _tries = new List<TryResult<T>>();
        protected MutableInt _mutableInt;
        
        public event D.ValueChangedDelegate OnTestValueUpdated;
        public event D.CompletedDelegate<T> OnTestCompleted;
        public event D.VoidDelegate OnTryFailed;

        protected abstract bool UseTimeLimit { get; }
        protected abstract int RepeatCount {get;}

        protected abstract void RecordTry(TryResult<T> tryRecord);

        public BaseTest(int[] values)
        {
            _mutableInt = new MutableInt(values, RepeatCount);
            _mutableInt.OnValueChanged += (newValue) =>
            {
                _tries.Add(new TryResult<T>(_tries.Count + 1));

                if (OnTestValueUpdated != null)
                    OnTestValueUpdated(newValue);
            };
            _mutableInt.OnMutationComplete += () =>
            {
                if (OnTestCompleted != null)
                    OnTestCompleted(_tries);
            };

            _tries.Add(new TryResult<T>(1));
        }

        

        public int TestValue { get { return _mutableInt.Value; } }

        public void StartTry()
        {
            _tryStart = DateTime.Now;
        }

        public bool IsTrying { get { return _tryStart != null; } }

        public void FinishTry(bool recordTry)
        {
            _lastTryTime = DateTime.Now - _tryStart.Value;

            if (UseTimeLimit && _lastTryTime > _tryTimeLimit)
            {
                if (OnTryFailed != null)
                    OnTryFailed();

                _tryStart = null;
                return;
            }

            if (recordTry)
            {
                var currentItem =   _tries[_tries.Count - 1];
                RecordTry(currentItem);
            }
            
            _mutableInt.Mutate();
            _tryStart = null;
        }       
    }

    public static class Utils
    {
        public static void SaveResults<T>(string fileName, List<TryResult<T>> testResults)
        {
            SaveResults<T>(fileName, testResults, DefaultConverter<T>);
        }

        public static void SaveResults<T>(string fileName,List<TryResult<T>> testResults,Func<T,string> tValueConverter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var result in testResults)
            {
                sb.AppendLine(String.Format("{0}\t{1}", result.Value, tValueConverter(result.TValue)));
            }

            using (StreamWriter outfile = new StreamWriter(fileName))
            {
                outfile.Write(sb.ToString());
            }
        }

        private static string DefaultConverter<T>(T val)
        {
            return val.ToString();
        }
    }

    public class FitsMistakesTest : BaseTest<int>
    {

        public FitsMistakesTest(int[] values) : 
            base(values)
        {

        }

        protected override bool UseTimeLimit
        {
            get { return true; }
        }

        protected override int RepeatCount
        {
            get { return FitsLawTestValues.TRIES; }
        }

        protected override void RecordTry(TryResult<int> tryRecord)
        {
            tryRecord.TValue++;
        }
    }


    public class FitsLawTestValues
    {
        public static readonly int[] ButtonSizes = { 3, 4, 5, 6, 7, 8, 10, 12, 14 };
        public static readonly int[] ButtonDistances = { 0, 20, 40, 60, 100, 150, 200, 250, 300, 350 };
        public const int TRIES = 8;
    }


    public class KhikDecisionTest : BaseTest<TimeSpan>
    {
        public KhikDecisionTest(int[] values)
            : base(values)
        {

        }
        protected override bool UseTimeLimit
        {
            get { return false; }
        }

        protected override int RepeatCount
        {
            get { return 1; }
        }

        protected override void RecordTry(TryResult<TimeSpan> tryRecord)
        {
            tryRecord.TValue = _lastTryTime;
        }
    }
}
