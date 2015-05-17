using Atomic.Core;

namespace Atomic.Loader
{
    /// <summary>
    /// An generic interface for converting textual data into a process model.
    /// </summary>
    public interface IDataConverter
    {
        /// <summary>
        /// Imports formatted text into a process model/
        /// </summary>
        /// <param name="sourceText">The text that defines a process model.</param>
        void Import(string sourceText);

        /// <summary>
        /// Exports the process model into formatted text.
        /// </summary>
        /// <returns></returns>
        string Export();

        /// <summary>
        /// Returns the model contained in the converter.
        /// </summary>
        IProcessModel Model { get; }

        /// <summary>
        /// Returns the file extension to use when saving to a file.
        /// </summary>
        string FileExtension { get; }
    }

    /// <summary>
    /// A generic element model that supports importing and exporting of the specified 
    /// type of element from the core library.  
    /// </summary>
    /// <typeparam name="T">the core element type to import and export</typeparam>
    public interface IElementModel<T> : IElement where T : IElement
    {
        new string ID { get; set; }

        T Element { get; set; }

        /*
        /// <summary>
        /// Imports a core element into this model using the specified process model.
        /// </summary>
        /// <param name="model">The process model containing this element model.</param>
        /// <param name="item">The core element to import into the model.</param>
        void Import(IProcessModel model, T item);

        /// <summary>
        /// Exports a core element from the model using the specified export registry.
        /// </summary>
        /// <param name="reg">The registry containing all the core element references in the process model.</param>
        /// <returns></returns>
        T Export(ExportRegistry reg);
         * */
    }

    public interface IFunctionModel : IElementModel<IFunction>
    {
        string AssemblyName { get; set; }

        string ModuleName { get; set; }

        string MethodName { get; set; }
    }

    public interface IConditionModel : IElementModel<ICondition>
    {
        string FunctionID { get; set; }

        string ConditionType { get; set; }

        string TaskID { get; set; }

        string ExpectedState { get; set; }

        string ValueID { get; set; }

        string ExpectedValue { get; set; }
    }

    /// <summary>
    /// A process model definition used to import and export a core process element.
    /// </summary>
    public interface IProcessModel
    {
        /// <summary>
        /// The system-generated identifer of the process element.
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// The name of the process element.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// A collection of event models for each event contained in the process element.
        /// </summary>
        /// <remarks>
        /// This includes the start and stop events of the process model.
        /// </remarks>
        EventModel[] Events { get; }

        /// <summary>
        /// A collection of task models for each task contained in the process element.
        /// </summary>
        TaskModel[] Tasks { get; }

        /// <summary>
        /// A collection of unique condition models associated with tasks and events
        /// within the process element, as well as the process element.  
        /// </summary>
        /// <remarks>
        /// The done condition of the process element is stored here.
        /// </remarks>
        ConditionModel[] Conditions { get; }

        /// <summary>
        /// A collection of unique value models associated with tasks and events within
        /// the process element, as well as the process element.
        /// </summary>
        ValueModel[] Values { get; }

        /// <summary>
        /// A collection of unique functions associated with tasks and conditions in the
        /// process element.
        /// </summary>
        FunctionModel[] Functions { get; }

        /// <summary>
        /// Imports a process element into the process model.
        /// </summary>
        /// <param name="process">The process element to import.</param>
        void Import(IProcess process);

        /// <summary>
        /// Exports a process element from the process model.
        /// </summary>
        /// <returns>The process element represented by the model.</returns>
        IProcess Export();

        ConditionModel AddCondition(ICondition cond);

        EventModel AddEvent(IEvent evt);

        FunctionModel AddFunction(IFunction func);

        TaskModel AddTask(ITask task);

        ValueModel AddValue(IValue value, string parameterType = "");

        ContainerModel AddContainer(IContainer container);
    }
}
