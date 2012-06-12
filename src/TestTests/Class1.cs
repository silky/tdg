using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace TestTests
{
	public class Class1
	{
		[Test]
		public void Test1 ()
		{
			Assert.AreEqual(1, 1);
		}


		[Test(Description="Always passes.")]
		public void Test2 ()
		{
			Assert.AreEqual(1, 0);
		}


		[Test(Description="Sample comment.{TDG=pref:exercise,weight:500}")]
		public void Test3 ()
		{
			Assert.AreEqual(1, 0);
		}
	}
}
