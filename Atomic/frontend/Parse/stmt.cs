﻿using System;
using System.Linq;
using System.Collections.Generic;
using Atomic_AST;
namespace Atomic_lang;

public partial class Parser 
{         
        private Statement parse_func_declaration(string name) {
		var args = this.parse_args();

		List<string> parameters = new List<string>();
		foreach (Expression arg in args)
		{
			if (arg.type != "Identifier")
			{
				this.error("inside func declaration parameters has to be identifiers\ngot => " + arg.type,at());
			}
			parameters.Add((arg as Identifier).symbol);
		}

		this.except(IonType.OpenBrace);
		List<Statement> body = new List<Statement>();

		while (at().type != IonType.EOF && at().type != IonType.CloseBrace)
		{
			body.Add(this.parse_statement());
		}

		this.except(IonType.CloseBrace);

		FuncDeclarartion func = new FuncDeclarartion();
		func.name = name; func.parameters = parameters; func.body = body;

		return func;
	}

	private Statement parse_var_declaration()
	{
		take();
		string id;
		bool locked = false;
		if (at().type == IonType.locked_kw)
		{
			take();
			id = except(IonType.id).value;
			locked = true;
		}
		else
		{
			id = except(IonType.id).value;
			if(this.at().type == IonType.OpenParen) {
				return this.parse_func_declaration(id);
			}
		}

		VarDeclaration declare = new VarDeclaration();

		// also TODO: instead of checking id types for everything we can do (parse id) but this is an expirement
		declare.locked = locked;
		declare.Id = id;

		// TODO: request ';'
		if (at().type != IonType.setter)
		{
			if (locked)
			{
				error("must asinge value to locked vars",at());
			}

			//TODO request type for vars like this
			declare.value = null;
			return declare;
		}

		else
		{
			take();

			declare.value = this.parse_expr();

			return declare;
		}

	}

	private Statement parse_return_stmt()
	{
		take();

		ReturnStmt stmt = new ReturnStmt();

		stmt.value = this.parse_expr();

		return stmt;
	}
	private Statement parse_use_stmt() {
		this.take();
		string path = "null";
		if(this.at().type == IonType.str_type) {
				path = this.at().value;	
				this.take();
		}
		else if(this.at().type == IonType.id) {
			path = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Protons/" + this.at().value.ToLower() + ".proton";
			this.take();
		}
		
		useStmt stmt = Create<useStmt>();
		stmt.path = path;
		
		
		return stmt;
	}
	//in the future use and using will be the sane kw (use) but for dev properses
	private Statement parse_using_stmt() {
		this.take();

		var name = this.except(IonType.id).value;

		useStmt stmt = Create<useStmt>();

		stmt.path = name;

		return stmt;
	}
}
