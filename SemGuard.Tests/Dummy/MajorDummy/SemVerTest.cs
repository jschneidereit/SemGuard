using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
	public class SemVerTest
	{
		public string Name() => "SemVerTest";

		public string Hello() => "Hello";

		public string Parameters(int x) => $"{x} + nothing = {x}";

		private void DoesNotExist(int x, int y, int z) { }
	}
}
