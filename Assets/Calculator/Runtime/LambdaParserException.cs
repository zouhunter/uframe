using System;

namespace UFrame.Calculator {
	
	/// <summary>
	/// The exception that is thrown when lambda expression parse error occurs
	/// </summary>
	public class LambdaParserException : Exception {

		/// <summary>
		/// Lambda expression
		/// </summary>
		public string Expression { get; private set; }

		/// <summary>
		/// Parser position where syntax error occurs 
		/// </summary>
		public int Index { get; private set; }

		public LambdaParserException(string expr, int idx, string msg)
			: base( String.Format("{0} at {1}: {2}", msg, idx, expr) ) {
			Expression = expr;
			Index = idx;
		}
	}
}
