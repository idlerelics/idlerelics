namespace Core
{
	/// <summary>
	/// An interface for the Observer Pattern. Any class that wants to "watch" an Observable
	/// object and react when it changes must implement this interface.
	///
	/// WHAT IS AN INTERFACE?
	/// An interface defines a set of methods that a class MUST implement. It's like a contract.
	/// The "I" prefix is a C# naming convention for interfaces (IObserver, IDisposable, etc.).
	///
	/// HOW TO USE:
	/// 1. Make your class implement IObserver: "class MyClass : IObserver"
	/// 2. Implement OnObjectChanged() to define what happens when the observed object changes
	/// 3. Call observable.AddObserver(this) to start watching, RemoveObserver(this) to stop
	/// </summary>
	public interface IObserver
	{
		/// <summary>
		/// Called automatically when an Observable object this observer is watching changes.
		/// The "observable" parameter tells you WHICH object changed, so one observer
		/// can watch multiple objects and tell them apart.
		/// </summary>
		void OnObjectChanged(Observable observable);
	}
}
