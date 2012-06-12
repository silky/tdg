using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Core;
using NUnit.Core.Extensibility;

namespace TDG
{
	[NUnitAddinAttribute(
		Type		= ExtensionType.Core,
		Name		= "Test-Driven Gym",
		Description	= "Test-Driven Gym (TDG) is a configurable framework for " +
			"presenting questions/demands, based on the failure of unit testing. It aims" +
			" to help the programmer remember/learn various things, including: exercise," +
			" language, maths, etc. Whatever you want, it can be configured to prompt for!"
	)]
	public class TdgEngine : IAddin
	{
		/// <summary>
		///		Installs this as an 'Event' based plugin.
		/// </summary>
		public bool Install (IExtensionHost host)
		{
			IExtensionPoint listeners = host.GetExtensionPoint("EventListeners");
			listeners.Install(new CoreListenerExtension());
			return true;
		} /// <Install (...) endp>
	}
}