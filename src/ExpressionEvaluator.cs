using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mathematical_Expression_Evaluator
{
	class Token
	{
	
		public static readonly Dictionary<string, TokenType> patterns
			= new Dictionary<string, TokenType> 
		{
			{ "[0-9]+", TokenType.Number },
			{ "\\*\\*|\\*|\\/|\\+|-", TokenType.Operator },
			{ "\\(", TokenType.LeftParenthesis },
			{ "\\)", TokenType.RightParenthesis }
		};
		
		public enum TokenType
		{
			Number,
			Operator,
			LeftParenthesis,
			RightParenthesis
		}
	
		readonly TokenType _type;
		readonly string _value;
	
		public Token(TokenType type, string val)
		{
			this._type = type;
			this._value = val;
		}
	
		public TokenType Type
		{
			get => _type;
		}
	
		public string Value
		{
			get => _value;
		}
		
		public override string ToString()
		{
			return _type + ": " + _value;
		}
	}
	
	class MatchComparer : IComparer<Match>
	{
		public int Compare(Match x, Match y)
		{
	        return x.Index - y.Index;
	    }
	}
	
	interface ITokenizer
	{
		Queue<Token> Tokenize(string expression);
	}
	
	class RegexTokenizer : ITokenizer
	{
		public Queue<Token> Tokenize(string expression)
		{
			var comparer = new MatchComparer();
			var matches = new SortedList<Match, Token>(comparer);
			
			foreach (var pair in Token.patterns)
			{
				foreach (Match match in Regex.Matches(expression, pair.Key))
				{
					var token = new Token(pair.Value, match.Value);
					matches.Add(match, token);
				}
			}
	
			return new Queue<Token>(matches.Values);
		}
	}
	
	class NotationConverter
	{
		private readonly Dictionary<string, int> operatorPrecedence 
			= new Dictionary<string, int> 
		{
			{ "**", 4 },
			{ "/", 3 },
			{ "*", 2 },
			{ "-", 1 },
			{ "+", 0 }
		};
			
		private bool TakesPrecedence(Token first, Token second)
		{
			return operatorPrecedence[first.Value] - operatorPrecedence[second.Value] >= 0;
		}
		
		public Queue<Token> ToPostfix(Queue<Token> infix)
		{
			Queue<Token> output = new Queue<Token>();
			Stack<Token> operators = new Stack<Token>();
			
			while (infix.Count > 0)
			{
				var token = infix.Dequeue();
	
				switch (token.Type) {
					case Token.TokenType.Number:
						output.Enqueue(token);
						break;
						
					case Token.TokenType.Operator:
						Token nextOperator;
	
						while (operators.TryPeek(out nextOperator) 
							&& nextOperator.Type != Token.TokenType.LeftParenthesis
							&& TakesPrecedence(nextOperator, token))
						{
							output.Enqueue(operators.Pop());
						}
	
						operators.Push(token);
						break;
	
					case Token.TokenType.LeftParenthesis:
						operators.Push(token);
						break;
	
					case Token.TokenType.RightParenthesis:
						Token nextOperator1;
						
						while (operators.TryPeek(out nextOperator1)
							   && nextOperator1.Type != Token.TokenType.LeftParenthesis)
						{
							output.Enqueue(operators.Pop());
						}
	
						if (operators.Count > 0)
						{
							operators.Pop();
						}
						break;
				}
			}
	
			while (operators.Count > 0)
			{
				output.Enqueue(operators.Pop());
			}
			
			return output;
		}
	}
	
	interface IEvaluator {
		int Evaluate(Queue<Token> tokens);
	}
	
	class PostfixEvaluator : IEvaluator
	{
		private readonly Dictionary<string, Func<int, int, int>>
			operatorFunctions = new Dictionary<string, Func<int, int, int>>
		{
			{ "**", (op1, op2) => (int) Math.Pow(op1, op2) },
			{ "*", (op1, op2) => op1 * op2 },
			{ "/", (op1, op2) => op1 / op2 },
			{ "+", (op1, op2) => op1 + op2 },
			{ "-", (op1, op2) => op1 - op2 }
		};
	
		public int Evaluate(Queue<Token> tokens)
		{
			var stack = new Stack<int>();
	
			foreach (var token in tokens) {
				switch(token.Type) {
					case Token.TokenType.Number:
						var intValue = int.Parse(token.Value);
						stack.Push(intValue);
						break;
	
					case Token.TokenType.Operator:
						var operand2 = stack.Pop();
						var operand1 = stack.Pop();
						var result = operatorFunctions[token.Value](operand1, operand2);
						stack.Push(result);
						break;
				}
			}
			
			return stack.Pop();
		}
	}
	
	class Program
	{
		static void Main(string[] args)
		{
			var expression = "5 ** (4/2)";
			
			var tokenizer = new RegexTokenizer();			
			var tokens = tokenizer.Tokenize(expression);
	
			var notationConverter = new NotationConverter();
			var postfix = notationConverter.ToPostfix(tokens);
	
			var evaluator = new PostfixEvaluator();
			var evaluation = evaluator.Evaluate(postfix);
	
			Console.WriteLine(expression + " = " + evaluation);
		}
		
	}
}