using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{

    public class ConditionMetView : AtomicView
    {
        private ICondition _condition = Undefined.Condition;

        public ICondition Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public override object Value
        {
            get { return Condition.Met; }
        }
    }

    public class TaskStateView : AtomicView
    {
        private IRunnable _task = Undefined.Task;

        public IRunnable Task
        {
            get { return _task; }
            set { _task = value; }
        }

        public override object Value
        {
            get { return Task.CurrentState; }
        }
    }

    public class ValueModifiedView : AtomicView
    {
        private IValue _compareValue = Undefined.Value;

        public IValue CompareValue
        {
            get { return _compareValue; }
            set { _compareValue = value; }
        }

        public override object Value
        {
            get { return CompareValue.Modified; }
        }
    }

    abstract public class ValueView<T> : AtomicValue, IValueView<T>
    {
        private IValue _sourceValue = Undefined.Value;

        public IValue SourceValue
        {
            get { return _sourceValue; }
            set { _sourceValue = value; }
        }

        abstract new public T Value { get; }
    }

    public class TextView : ValueView<string>
    {
        public override string Value
        {
            get { return SourceValue.Value.ToString(); }
        }
    }

    public class NumberView : ValueView<double>
    {
        public override double Value
        {
            get 
            {
                double d = 0.0d;
                string textValue = SourceValue.Value.ToString();
                
                bool valid = Double.TryParse(textValue, out d);

                return d;
            }
        }
    }

    public class IndexView : ValueView<int>
    {
        public override int Value
        {
            get
            {
                int i = 0;
                string textValue = SourceValue.Value.ToString();

                bool valid = Int32.TryParse(textValue, out i);

                return i;
            }
        }
    }

    public class BooleanView : ValueView<bool>
    {
        public override bool Value
        {
            get { return SourceValue != Undefined.Value; }
        }
    }

    public class ListView : ValueView<object[]>
    {
        public override object[] Value
        {
            get 
            { 
                List<object> objList = new List<object>();
                if (SourceValue.Value is System.Collections.ICollection)
                {
                    System.Collections.ICollection coll = (System.Collections.ICollection)SourceValue.Value;
                    System.Collections.IEnumerator enu = coll.GetEnumerator();
                    do 
                    {
                        objList.Add(enu.Current);
                    } while (enu.MoveNext());
                }
                else
                {
                    objList.Add(SourceValue.Value);
                }

                return objList.ToArray();
            }
        }
    }

    public class MapValue : ValueView<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Value
        {
            get 
            {
                IDictionary<string, object> map = new Dictionary<string, object>();
                if (SourceValue.Value is IValue[])
                {
                    IValue[] values = (IValue[])SourceValue.Value;
                    foreach (IValue v in values)
                    {
                        map[v.Name] = v.Value;
                    }
                }
                else if (SourceValue.Value is IValue)
                {
                    IValue v = (IValue)SourceValue.Value;
                    map.Add(v.Name, v.Value);
                }
                else if (SourceValue.Value is System.Collections.ICollection)
                {
                    System.Collections.ICollection valueCol = (System.Collections.ICollection)SourceValue.Value;
                    System.Collections.IEnumerator valueEn = valueCol.GetEnumerator();
                    int index = 1;
                    do
                    {
                        map.Add(index.ToString(), valueEn.Current);
                        index++;
                    } while (valueEn.MoveNext());
                }
                else
                {
                    map.Add("1", SourceValue.Value);
                }

                return map;
            }
        }
    }

    public class ListItemView : ValueView<object>
    {
        public new ListView SourceValue { get; set; }

        public IndexView Index { get; set; }

        public override object Value
        {
            get { return SourceValue.Value[Index.Value]; }
        }
    }

    public class MapItemView : ValueView<object>
    {
        public new MapValue SourceValue { get; set; }

        public TextView Key { get; set; }

        public override object Value
        {
            get { return SourceValue.Value[Key.Value]; }
        }
    }

    public class TaskView : ValueView<IRunnable>, IRunnable
    {
        public override IRunnable Value
        {
            get
            {
                if (SourceValue is IRunnable)
                {
                    return (IRunnable)SourceValue.Value;
                }
                else
                {
                    return Undefined.Task;
                }
            }
        }

        public void Run()
        {
            Value.Run();
        }

        public RunState CurrentState
        {
            get { return Value.CurrentState; }
        }

        public TaskFunction RunFunction
        {
            get { return Value.RunFunction; }
            set { }
        }

        public IValue[] Values
        {
            get { return Value.Values; }
            set { }
        }

        public IValue GetValue(string name)
        {
            return Value.GetValue(name);
        }
    }

}
