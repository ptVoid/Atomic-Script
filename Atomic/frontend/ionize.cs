﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Ttype;
namespace Atomic;

public class Ionizing
{
	public void error(string message)
	{
		Console.BackgroundColor = ConsoleColor.Red;
		Console.ForegroundColor = ConsoleColor.Yellow;

		Console.WriteLine(message + "\nat => line:{0}, column:{1}", line, column);
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;

		Global.Var.error = true;
		move();

	}
	public Ionizing(string code)
	{
		atoms = code;
		ions.Clear();
	}
	public static string atoms { get; set; }
	public string[] keywords = { "set", "locked", "Null", "func", "return", "null", "if", "else"};
	public static List<(string value, TokenType type)> ions = new List<(string value, TokenType type)>();
	public static int column = 1;
	public static int line = 1;


	public bool isAllowedID(char x)
	{
		//only english && langs that has upper and lower chars is allowed
		return "_qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789".Contains(x);
	}
	public bool isOp(char x)
	{
		return "+-/*%<=".Contains(x);
	}

	public char current_atom()
	{
		if (atoms.Length > 0)
		{
			return atoms[0];
		}
		return ';';
	}


	public bool isBool(string x)
	{
		if (x == "true" || x == "false")
		{
			return true;
		}
		return false;
	}


	public char after_current_atom()
	{
		if (atoms.Length > 1)
		{
			return atoms[1];
		}
		return ';';
	}


	public bool IsSkippable(char n)
	{
		string x = n.ToString();
		return x == " " || x == "\t" || x == "\r" || x == ";";
	}

	public bool IsLine(string x)
	{
		return x == "\n";
	}

	public bool isNum(char n)
	{
		string x = n.ToString();
		//getting int by checking if the char code of the string is begger or smaller or equal to (0-9) charcode
		return "0123456789".Contains(x);
	}

	public bool isKeyword(string x)
	{

		return keywords.Contains(x);
	}


	public TokenType KeywordType(string x)
	{
		switch (x)
		{
			case "set":
				return TokenType.set;
			case "locked":
				return TokenType.locked;
			case "func":
				return TokenType.func;
			case "return":
				return TokenType.return_kw;
			case "null":
				return TokenType.Null;
			case  "if":
				return TokenType.if_kw;
			case "else":
				return TokenType.else_kw;
			default:
				error("unknown error please report this! code:unknown_02?" + x);
				return TokenType.Null;
		}
	}
	public static void move(int by = 1)
	{
		while (by != 0)
		{
			try
			{
				atoms = atoms.Substring(1);
			}
			catch
			{

			}
			column++;
			by--;
		}
	}
	//todo rewritw the ionizer
	public List<(string value, TokenType type)> ionize()
	{

		while (atoms.Length > 0)
		{
			switch (current_atom())
			{
				case '(':
					ions.Add(("(", TokenType.OpenParen));
					move();
					continue;
				case ')':
					ions.Add((")", TokenType.CloseParen));
					move();
					continue;
				case '{':
					ions.Add(("{", TokenType.OpenBrace));
					move();
					continue;
				case '}':
					ions.Add(("}", TokenType.CloseBrace));
					move();
					continue;
				case '[':
					ions.Add(("[", TokenType.OpenBracket));
					move();
					continue;
				case ']':
					ions.Add(("]", TokenType.CloseBracket));
					move();
					continue;
				case '.':
					ions.Add((".", TokenType.Dot));
					move();
					continue;
				case ',':
					ions.Add((",", TokenType.Comma));
					move();
					continue;
				case ':':
					ions.Add((":", TokenType.Colon));
					move();
					continue;
				case '#':
					move();
					while (atoms.Length > 0 && current_atom().ToString() != "\n")
					{
						move();
					}
					continue;
				default:
				    
					if (IsLine(current_atom().ToString()))
					{
						line++;
						column = 1;

						
						move();
					}


					else if (IsSkippable(current_atom()))
					{
						move();
					}


					//detecting strings
					else if (current_atom() == '"')
					{
						string res = "";
						move();
						try
						{
							while (current_atom() != '"' && atoms.Length > 0)
							{
								res += current_atom();
								move();
							}
						}
						catch
						{
							error("reached end of file and didnt finish string");
						}
						if (current_atom() != '"')
						{
							error("unfinshed string");
						}

						ions.Add((res, TokenType.str));
						move();
					}

					

					else if (isNum(current_atom()))
					{
						string res = "";

						while (isNum(current_atom()))
						{
							res += current_atom();
							move();
						}
						ions.Add((res, TokenType.num));
					}




					else if (isAllowedID(current_atom()))
					{
						string res = "";

						while (isAllowedID(current_atom()))
						{
							res += current_atom();
							move();
						}
						if (isKeyword(res))
						{
							ions.Add((res, KeywordType(res)));

						}
						else if (isBool(res))
						{
							ions.Add((res, TokenType.Bool));
						}
						else
						{
							ions.Add((res, TokenType.id));

						}
					}


					else if (isOp(current_atom()))
					{
						string res = current_atom().ToString();
						ions.Add((res, TokenType.op));
						move();
					}

					else if (current_atom() == '>')
					{
						move();
						if (current_atom() == '>')
						{
							ions.Add((">>", TokenType.setter));
							move();
						}
						else
						{
							ions.Add((">", TokenType.op));
						}

					}
					else if (current_atom() == '&')
					{
						move();
						if (current_atom() == '&')
						{
							ions.Add(("&&", TokenType.op));
							move();
						}
						else
						{
							ions.Add(("&", TokenType.op));
						}
					}
					else if (current_atom() == '|')
					{
						move();
						if (current_atom() == '|')
						{
							ions.Add(("||", TokenType.op));
							move();
						}
						else
						{
							ions.Add(("|", TokenType.op));
						}
					}
					else
					{
						error("unknown char " + current_atom());
					}
					continue;
			}


		}


		ions.Add(("END", TokenType.EOF));
		return ions;
	}
}
