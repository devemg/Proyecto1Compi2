﻿using com.Analisis;
using com.Analisis.Util;
using Proyecto1Compi2.com.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto1Compi2.com.AST
{
	class Rollback : Sentencia
	{
		public Rollback(int linea, int columna) : base(linea, columna)
		{
		}

		public override object Ejecutar(TablaSimbolos tb)
		{
			Analizador.Clear();
			String chi = HandlerFiles.AbrirArchivo("data\\principal.chison");
			if (chi != null)
			{
				if (Analizador.AnalizarChison(chi))
				{
					Console.WriteLine("ARCHIVO CARGADO CON EXITO");
				}
				else
				{
					Console.WriteLine("ARCHIVO CARGADO CON ERRORES");
					//Analizador.Errors.MostrarCabecera();
					//Analizador.Errors.MostrarDatos();
				}
			}
			return null;
		}
	}
}
