using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Xml;

using NUnit.Core;
using NUnit.Core.Extensibility;

namespace TDG
{
	public class CoreListenerExtension : NUnit.Core.EventListener
	{
		/// <summary>
		///		List of all the items that have been read from
		///		the XML, the format is:
		/// 
		///			0   : type
		///			1-n : type-specific
		/// 
		/// </summary>
		private List<string[]>
			items = new List<string[]>();


		/// <summary>
		///		Used for getting random indexes of the 'Items'.
		/// </summary>
		private Random
			random = new Random();
		

		/// <summary>
		///		List of all the results we add on, based on all
		///		the failures that we've encountered.
		/// </summary>
		private List<TestResult>
			_results = new List<TestResult>();


        /// <summary>
        ///     Map of items along with the number of that particular type, used when
        ///     a particular test prefers a certain category.
        /// </summary>
		private Dictionary<string, int[]>
			itemMap = new Dictionary<string, int[]>();


		/// <summary>
		///		Loads the actions from '_dd.xml'.
		/// </summary>
		public CoreListenerExtension ()
		{
			try {
				string xmlPath = Path.Combine(Environment.CurrentDirectory, "addins" + Path.DirectorySeparatorChar + "tdg.xml");

				if( !File.Exists(xmlPath) ){
					//	Do nothing.
					return;
				}

				XmlDocument document = new XmlDocument();
				document.LoadXml(File.ReadAllText(xmlPath));

				string use = document.DocumentElement.GetAttribute("use");

				if( string.IsNullOrEmpty(use) ){
					return;
				}

				string[] toUse = use.Split(new char[1]{','}, StringSplitOptions.RemoveEmptyEntries);

				Dictionary<string, string> map = new Dictionary<string,string>();
				
				foreach(string s in toUse){
					map.Add(s, null);
				}

				foreach(XmlElement node in document.DocumentElement.SelectNodes("//itemgroup")){
					string type = node.GetAttribute("type");

					if( !map.ContainsKey(type) ){
						continue;
					}

					this.itemMap.Add(type, new int[2]{ this.items.Count, 0 });

					foreach(XmlElement item in node.SelectNodes(".//item")){
						string[] bits = new string[5];

						bits[0] = type;
						bits[1] = item.GetAttribute("description");
						bits[2] = item.GetAttribute("weight");
						bits[3] = item.GetAttribute("min");
						bits[4] = item.GetAttribute("max");

						this.items.Add(bits);
					}

					this.itemMap[type][1] = this.items.Count;
				}
			} catch(Exception){
				//	If we fail to load we will just be silent.
			}
		} /// <CoreListenerExtension () endp>


		/// <summary>
		///		For each test that finished, if it failed, we need to
		///		do some work.
		/// </summary>
		public void TestFinished (TestResult result)
		{
			if( ! result.IsFailure ){
				return;
			}

			TestName	name	= new TestName();
			string[]	bits	= this.items[this.random.Next(0, this.items.Count)];

			try {
				if( !string.IsNullOrEmpty(result.Description) ){
					string	prefix		= "{TDG=";
					int		startIndex	= result.Description.IndexOf(prefix);

					if( startIndex > -1 ){
						int		endIndex	= result.Description.IndexOf("}", startIndex);
						int		actualSI	= startIndex + prefix.Length;
						string	component	= result.Description.Substring(actualSI, endIndex - actualSI);

						string[] componentBits = component.Split(new char[1]{','}, StringSplitOptions.RemoveEmptyEntries);

						foreach(string s in componentBits){
							string[] kvp = s.Split(':');

							switch(kvp[0].ToLower()){
								case "pref":
									bits = this.items[this.random.Next(this.itemMap[kvp[1]][0], this.itemMap[kvp[1]][1])];
									break;

								case "weight":
									//	'weight' is at 'bits[2]', so we will adjust it if they have
									//	specified so.
									bits[2] = kvp[1];
									break;
							}
						}
					}
				}
			} catch(Exception){
				// we can ignore this exception (it amounts to ignoring any customisation they've done
				// of the execution; perhaps the data was not in the correct format, either way, it is
				// not critical.
			}

			string type = bits[0].ToLower();

			name.FullName = string.Format("TDG action of type '{0}'.", type);

			string actionDetails = bits[1];

			switch(type){
				case "exercise":
					if( !string.IsNullOrEmpty(bits[2]) &&
						!string.IsNullOrEmpty(bits[3]) &&
						!string.IsNullOrEmpty(bits[4])
						) {

						// Need to set the variables into the string

						actionDetails = string.Format(
							actionDetails,
							this.random.Next(
								int.Parse(bits[2]) * int.Parse(bits[3]),
								int.Parse(bits[2]) * int.Parse(bits[4]) + 1
							)
						);
					}
					break;

                default:
					// Nothing to do, just display
					break;
			}


			TestResult test = new TestResult(name);

			// For NUnit-2.5.2.9222
			// test.SetResult(ResultState.Failure, new Exception(actionDetails));

			// For NUnit-2.6.0.12051
			test.SetResult(ResultState.Failure, new Exception(actionDetails), FailureSite.Test);
			_results.Add(test);
		} /// <TestFinished (TestResult) endp>


		/// <summary>
		///		When the complete rnning of the test is done, let us add
		///		on all the relevant items.
		/// </summary>
		public void RunFinished (TestResult	result )
		{
			if( !result.IsFailure ){
				return;
			}

			foreach(TestResult t in this._results){
				result.AddResult(t);
			}
		} /// <RunFinished (TestResult) endp>


		#region *  Un-implemented Interface Members         *
		public void TestStarted			(TestName	testName)	{}
		public void RunStarted			(string name, int testCount) {}
		public void RunFinished			(Exception	exception )	{}
		public void SuiteStarted		(TestName	testName)	{}
		public void SuiteFinished		(TestResult	result)		{}
		public void UnhandledException	(Exception	exception )	{}
		public void TestOutput			(TestOutput	testOutput)	{}
		#endregion
	}
}
