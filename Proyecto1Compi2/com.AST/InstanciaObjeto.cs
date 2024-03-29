﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.Analisis;
using com.Analisis.Util;
using Proyecto1Compi2.com.db;
using Proyecto1Compi2.com.Util;

namespace Proyecto1Compi2.com.AST
{
	class InstanciaObjeto:Expresion
	{
		string nombreUserType;
		List<Expresion> expresiones;

		public InstanciaObjeto(string nombreUserType, List<Expresion> expresiones,int linea,int columna):base(linea,columna)
		{
			this.nombreUserType = nombreUserType;
			this.expresiones = expresiones;
		}

		public string NombreUserType { get => nombreUserType; set => nombreUserType = value; }
		internal List<Expresion> Expresiones { get => expresiones; set => expresiones = value; }

		public override TipoOperacion GetTipo(TablaSimbolos ts,Sesion sesion)
		{
			return TipoOperacion.InstanciaObjeto;
		}

		public override object GetValor(TablaSimbolos ts,Sesion sesion)
		{
			//VALIDANDO BASEDATOS
			if (sesion.DBActual != null)
			{
				BaseDatos db = Analizador.BuscarDB(sesion.DBActual);
				if (db.ExisteUserType(this.nombreUserType))
				{
					UserType ut = db.BuscarUserType(this.nombreUserType);
					Objeto nuevaInstancia = new Objeto(ut);
					int indice = 0;
					object resExp;
					if (ut.Atributos.Count == this.expresiones.Count)
					{
						foreach (KeyValuePair<string, TipoObjetoDB> atributo in ut.Atributos)
						{
							resExp = Expresiones.ElementAt(indice).GetValor(ts,sesion);
							if (resExp != null)
							{
								if (resExp.GetType() == typeof(ThrowError))
								{
									return resExp;
								}
							}
							if (Datos.IsTipoCompatibleParaAsignar(atributo.Value, resExp))
							{
								resExp = Datos.CasteoImplicito(atributo.Value, resExp,ts,sesion,Linea,Columna);
								if (resExp != null)
								{
									if (resExp.GetType() == typeof(ThrowError))
									{
										return resExp;
									}
									nuevaInstancia.Atributos.Add(atributo.Key, resExp);
								}
								
							}
							else
							{
								if (resExp.Equals("null") && atributo.Value.Tipo == TipoDatoDB.OBJETO)
								{
									object valorPre = Declaracion.GetValorPredeterminado(atributo.Value, sesion, Linea, Columna);
									if (valorPre != null)
									{
										if (valorPre.GetType() == typeof(ThrowError))
										{
											return valorPre;
										}
										nuevaInstancia.Atributos.Add(atributo.Key, valorPre);
									}
								}
								else {
									return new ThrowError(Util.TipoThrow.Exception,
																		"Los atributos no corresponden al tipo '" + this.nombreUserType + "'",
																		Linea, Columna);
								}
							}
							indice++;
						}
					}
					else {
						return new ThrowError(Util.TipoThrow.Exception,
							"Los atributos no corresponden en numero al tipo '" + nombreUserType + "'",
							Linea, Columna);
					}
					//asignando la nueva instancia despues de todas las validaciones
					return nuevaInstancia;
				}
				else
				{
					return new ThrowError(Util.TipoThrow.Exception,
					"El user Type '" + this.nombreUserType + "' no existe",
					Linea, Columna);
				}
			}
			else
			{
				return new ThrowError(Util.TipoThrow.UseBDException,
					"No se puede ejecutar la sentencia porque no hay una base de datos seleccionada",
					Linea, Columna);
			}
		}
	}
}
