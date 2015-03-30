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

    abstract public class ValueView<T> : AtomicView, IValueView<T>
    {
        private IValue _sourceValue = Undefined.Value;

        public IValue SourceValue
        {
            get { return _sourceValue; }
            set 
            { 
                _sourceValue = value;
                if (Name == ElementName + GetHashCode()) Name = SourceValue.Name;
            }
        }

        public bool Required { get; set; }

        public override object Value
        {
            get { return ViewValue; }
        }

        abstract public T ViewValue { get; } 
    }

    public class TextView : ValueView<string>
    {
        public override string ViewValue
        {
            get { return SourceValue.Value.ToString(); }
        }
    }

    public class NumberView : ValueView<double>
    {
        public override double ViewValue
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
        public override int ViewValue
        {
            get
            {
                if (SourceValue.Value == null) return 0;

                int i = 0;
                string textValue = SourceValue.Value.ToString();

                double d = 0.0;
                bool valid = Double.TryParse(textValue, out d);

                if (valid)
                {
                    i = (int)Math.Round(d);
                }
                //valid = Int32.TryParse(textValue, out i);

                return i;
            }
        }
    }

    public class BooleanView : ValueView<bool>
    {
        public override bool ViewValue
        {
            get { return SourceValue != Undefined.Value; }
        }
    }

    public class ListView : ValueView<object[]>
    {
        public override object[] ViewValue
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
        public override IDictionary<string, object> ViewValue
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

        public override object ViewValue
        {
            get 
            { 
                int index = Index.ViewValue;
                return SourceValue.ViewValue[index]; 
            }
        }
    }

    public class MapItemView : ValueView<object>
    {
        public new MapValue SourceValue { get; set; }

        public TextView Key { get; set; }

        public override object ViewValue
        {
            get { return SourceValue.ViewValue[Key.ViewValue]; }
        }
    }

    public class TaskView : ValueView<IRunnable>, IRunnable
    {
        public override IRunnable ViewValue
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
            ViewValue.Run();
        }

        public RunState CurrentState
        {
            get { return ViewValue.CurrentState; }
        }

        public IFunction RunFunction
        {
            get { return ViewValue.RunFunction; }
            set { }
        }

        public string FunctionText
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValueView[] Inputs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValueView GetInput(string name)
        {
            throw new NotImplementedException();
        }

        public IValue[] Outputs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValue GetOutput(string name)
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }


        public IProcess Process
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class ProcessView : ValueView<IProcess>, IProcess
    {
        public override IProcess ViewValue
        {
            get 
            {
                IProcess p = Undefined.Process;
                if (SourceValue is IProcess)
                {
                    p = (IProcess)SourceValue;
                }

                return p;
            }
        }

        public IEvent StartEvent
        {
            get { throw new NotImplementedException(); }
        }

        public IEvent StopEvent
        {
            get { throw new NotImplementedException(); }
        }

        public IEvent[] Events
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEvent GetEvent(string name)
        {
            throw new NotImplementedException();
        }

        public ITask[] Tasks
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public ITask GetTask(string name)
        {
            throw new NotImplementedException();
        }

        public ICondition DoneCondition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IContainer GetContainer(Type taskType)
        {
            throw new NotImplementedException();
        }

        public void SetContainer(Type taskType, IContainer container)
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public RunState CurrentState
        {
            get { throw new NotImplementedException(); }
        }

        public IFunction RunFunction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string FunctionText
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValueView[] Inputs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValueView GetInput(string name)
        {
            throw new NotImplementedException();
        }

        public IValue[] Outputs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValue GetOutput(string name)
        {
            throw new NotImplementedException();
        }

        public void ResetValues()
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }


        public IProcess Process
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
