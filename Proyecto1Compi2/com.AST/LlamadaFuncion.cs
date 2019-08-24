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

		public override object Ejecutar(Sesion sesion, TablaSimbolos ts)
		{
			String llave = getLlave(ts);
			if (Analizador.ExisteFuncion(llave))
			{
				Funcion funcion = Analizador.BuscarFuncion(llave);
				List<object> valores = new List<object>();
				//VALIDAR PARAMETROS 
				if (funcion.Parametros.Count == parametros.Count)
				{
					int contador = 0;
					foreach (Parametro vals in funcion.Parametros) {
						if (Datos.IsTipoCompatibleParaAsignar(vals.Tipo, parametros.ElementAt(contador).GetValor(ts)))
						{
							object nuevoDato = Datos.CasteoImplicito(vals.Tipo.Tipo, parametros.ElementAt(contador).GetValor(ts));
							valores.Add(nuevoDato);
						}
						else
						{
							return new ThrowError(Util.TipoThrow.Exception,
								"No se puede asignar el valor a la variable '"+vals.Nombre+"'",
								Linea, Columna);
						}
					}
				}
				else {
					return new ThrowError(Util.TipoThrow.Exception,
					"La cantidad de parámetros es incorrecta",
					Linea, Columna);
				}

				funcion.pasarParametros(valores);
				object res = funcion.Ejecutar(sesion, ts);
				if (res!=null) {
					return res;
				}
				funcion.LimpiarParametros();
			}
			else
			{
				return new ThrowError(Util.TipoThrow.Exception,
					"La función '" + llave + "' no existe",
					Linea, Columna);
			}
			return null;
		}

		internal string getLlave(TablaSimbolos ts)
		{
			StringBuilder llave = new StringBuilder();
			llave.Append(nombre + "(");
			int contador = 0;
			foreach (Expresion ex in parametros)
			{
				TipoOperacion t= ex.GetTipo(ts);
				if (t == TipoOperacion.Numero)
				{
					if (ex.GetValor(ts).ToString().Contains("."))
					{
						llave.Append("double");
					}
					else
					{
						llave.Append("int");
					}
				}
				else {
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
	}
}
