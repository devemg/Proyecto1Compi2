﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.Analisis.Util;
using Proyecto1Compi2.com.db;

namespace Proyecto1Compi2.com.AST
{
	class ElseIf:Sentencia
	{
		Condicion condicion;
		List<Sentencia> sentencias;
		public ElseIf(Condicion condicion, List<Sentencia> sentencias,int linea,int columna):base(linea,columna)
		{
			this.condicion = condicion;
			this.sentencias = sentencias;
		}

		internal Condicion Condicion { get => condicion; set => condicion = value; }
		internal List<Sentencia> Sentencias { get => sentencias; set => sentencias = value; }

		public override object Ejecutar(Sesion sesion, TablaSimbolos tb)
		{
			TablaSimbolos tlocal = new TablaSimbolos(tb);
			object res =EjecutarSentencias(sentencias, sesion, tlocal);
			if (res != null) return res;
			return null;
		}

		public static object EjecutarSentencias(List<Sentencia> MisSentencias, Sesion sesion, TablaSimbolos tsLocal)
		{
			object respuesta;
			foreach (Sentencia sentencia in MisSentencias)
			{
				respuesta = sentencia.Ejecutar(sesion, tsLocal);
				if (respuesta != null)
				{
					if (respuesta.GetType() == typeof(ThrowError))
					{
						return respuesta;
					}
					else
					{
						//EVALUAR SI ES RETURN, BREAK O CONTINUE
					}
				}
			}
			return null;
		}
	}
}
