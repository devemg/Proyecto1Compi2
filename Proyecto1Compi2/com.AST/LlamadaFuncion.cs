﻿using com.Analisis;
using com.Analisis.Util;
using Proyecto1Compi2.com.db;
using Proyecto1Compi2.com.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto1Compi2.com.AST
{
	class LlamadaFuncion : Sentencia
	{
		string nombre;
		List<Expresion> parametros;

		public LlamadaFuncion(string nombre, List<Expresion> parametros,int linea,int columna):base(linea,columna)
		{
			this.nombre = nombre;
			this.parametros = parametros;
		}

		public string Nombre { get => nombre; set => nombre = value; }
		internal List<Expresion> Parametros { get => parametros; set => parametros = value; }

		public override object Ejecutar(TablaSimbolos ts,Sesion sesion)
		{
			String llave = getLlave(ts,sesion);
			//FUNCIONES NATIVAS
			//***************************************************************
			if (llave.Equals("today()")) {
				return new MyDateTime(TipoDatoDB.DATE, DateTime.Today);
			} else if (llave.Equals("now()")) {
				return new MyDateTime(TipoDatoDB.TIME, DateTime.Now);
			}
			//***************************************************************
			if (Analizador.ExisteFuncion(llave))
			{
				Funcion funcion = Analizador.BuscarFuncion(llave);
				List<object> valores = new List<object>();
				//VALIDAR PARAMETROS 
				if (funcion.Parametros.Count == parametros.Count)
				{
					int contador = 0;
					foreach (Parametro vals in funcion.Parametros) {
						object posibleValor = parametros.ElementAt(contador).GetValor(ts, sesion);
						if (posibleValor!=null) {
							if (posibleValor.GetType()==typeof(ThrowError)) {
								return posibleValor;
							}
							if (Datos.IsTipoCompatibleParaAsignar(vals.Tipo,posibleValor))
							{
								object nuevoDato = Datos.CasteoImplicito(vals.Tipo, posibleValor,
									ts, sesion, Linea, Columna);
								if (nuevoDato != null)
								{
									if (nuevoDato.GetType() == typeof(ThrowError))
									{
										return nuevoDato;
									}
									valores.Add(nuevoDato);
								}
							}
							else
							{
								return new ThrowError(Util.TipoThrow.Exception,
									"No se puede asignar el valor a la variable '" + vals.Nombre + "'",
									Linea, Columna);
							}
						}
						
						contador++;
					}
				}
				else {
					return new ThrowError(Util.TipoThrow.Exception,
					"La cantidad de parámetros es incorrecta",
					Linea, Columna);
				}

				funcion.pasarParametros(valores);
				object res = funcion.Ejecutar(ts,sesion);
				if (res!=null) {
					return res;
				}
				funcion.LimpiarParametros();
				return res;
			}
			else
			{
				return new ThrowError(Util.TipoThrow.Exception,
					"La función '" + GetLlaveExterna(ts,sesion) + "' no existe",
					Linea, Columna);
			}
		}

		internal string GetLlaveExterna(TablaSimbolos ts, Sesion sesion)
		{
			StringBuilder llave = new StringBuilder();
			llave.Append(nombre + "(");
			int contador = 0;
			foreach (Expresion ex in parametros)
			{
				TipoOperacion t = ex.GetTipo(ts, sesion);
				if (t == TipoOperacion.Numero)
				{
					if (ex.GetValor(ts, sesion).ToString().Contains("."))
					{
						llave.Append("double");
					}
					else
					{
						llave.Append("int");
					}
				}
				else
				{
					llave.Append(t.ToString().ToLower());
				}
				if (contador < this.parametros.Count - 1)
				{
					llave.Append(",");
				}
				contador++;
			}
			llave.Append(")");
			return llave.ToString();
		}

		internal string getLlave(TablaSimbolos ts, Sesion sesion)
		{
			StringBuilder llave = new StringBuilder();
			llave.Append(nombre + "(");
			int contador = 0;
			foreach (Expresion ex in parametros)
			{
				ex.GetValor(ts,sesion);
				TipoOperacion t = ex.GetTipo(ts, sesion);
					llave.Append(t.ToString().ToLower());
				if (contador < this.parametros.Count - 1)
				{
					llave.Append(",");
				}
				contador++;
			}
			llave.Append(")");
			return llave.ToString();
		}
	}
}
