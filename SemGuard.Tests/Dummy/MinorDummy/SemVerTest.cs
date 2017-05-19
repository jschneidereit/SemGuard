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

		public string Parameters(int x, int y) => $"{x} + {y} = {x + y}";

		public string Goodbye() => "See ya";

		private void DoesNotExist(int x, int y) { } //private api removal here
	}
}
