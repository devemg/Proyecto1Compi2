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
	class Funcion : Sentencia
	{
		string nombre;
		List<Parametro> parametros;
		List<object> valoresParametros;
		TipoObjetoDB tipoRetorno;
		List<Sentencia> sentencias;

		public string Nombre { get => nombre; set => nombre = value; }
		public List<Parametro> Parametros { get => parametros; set => parametros = value; }
		public TipoObjetoDB TipoRetorno { get => tipoRetorno; set => tipoRetorno = value; }

		public Funcion(string nombre, List<Parametro> parametros, TipoObjetoDB retorno, List<Sentencia> sent, int linea, int columna) : base(linea, columna)
		{
			this.nombre = nombre;
			this.parametros = parametros;
			this.tipoRetorno = retorno;
			this.sentencias = sent;
		}

		internal void pasarParametros(List<object> parametros)
		{
			valoresParametros = parametros;
		}

		public override object Ejecutar(TablaSimbolos ts,Sesion sesion)
		{
			TablaSimbolos local = new TablaSimbolos(ts);
			int contador = 0;
			object RETORNO = null;
			//AGREGANDO PARAMETROS
			foreach (Parametro param in parametros)
			{
				if (!local.ExisteSimboloEnAmbito(param.Nombre))
				{
					local.AgregarSimbolo(new Simbolo(param.Nombre, valoresParametros.ElementAt(contador), param.Tipo, Linea, Columna));
				}
				else
				{
					//ERROR
					return new ThrowError(TipoThrow.Exception,
						"La variable '" + param.Nombre + "' ya existe",
						Linea, Columna);
				}
				contador++;
			}
			//EJECUTANDO SENTENCIAS ******************************************************************
			object respuesta;
			foreach (Sentencia sentencia in sentencias)
			{
				respuesta = sentencia.Ejecutar(local,sesion);
				if (respuesta != null)
				{
					if (respuesta.GetType() == typeof(ThrowError))
					{
						Analizador.ErroresCQL.Add(new Error((ThrowError)respuesta));
					}
					else if (respuesta.GetType() == typeof(List<ThrowError>))
					{
						//AGREGANDO ERRORES A LISTA PRINCIPAL
						List<ThrowError> errores = (List<ThrowError>)respuesta;
						foreach (ThrowError error in errores)
						{
							Analizador.ErroresCQL.Add(new Error(error));
						}
					}
					else if (respuesta.GetType() == typeof(Return))
					{
						Return ret = (Return)respuesta;
						respuesta = ret.ValoresRetornados;
						if (respuesta.GetType() == typeof(ThrowError)) return respuesta;
						List<object> valoresRetornados = (List<object>)respuesta;
						if (valoresRetornados.Count == 1)
						{
							RETORNO = valoresRetornados.ElementAt(0);
						}
						else if (this.tipoRetorno != null)
						{
							return new ThrowError(TipoThrow.NumerReturnsException,
								"La cantidad de valores retornados es incorrecta, solo se puede retornar un valor",
								Linea, Columna);
						}
						break;
					} else if (respuesta.GetType()==typeof(Throw)) {
						return respuesta;
					}
					else if (respuesta.GetType() == typeof(ResultadoConsulta))
					{
						Analizador.ResultadosConsultas.Add(((ResultadoConsulta)respuesta).ToString());
					}
					else if(respuesta.GetType()==typeof(Break)|| respuesta.GetType() == typeof(Continue))
					{
						//break - continue
						Sentencia sent = (Sentencia)respuesta;
						Analizador.ErroresCQL.Add(new Error(TipoError.Semantico,
							"La sentencia no está en un bloque de código adecuado",
							sent.Linea, sent.Columna));
					}
				}
			}
			//******************************************************************************************
			//EVALUAR RETORNO
			if (RETORNO != null)
			{
				TipoObjetoDB ti = Datos.GetTipoObjetoDB(RETORNO);
				if (Datos.IsTipoCompatibleParaAsignar(this.tipoRetorno, RETORNO))
				{
					return Datos.CasteoImplicito(this.tipoRetorno, RETORNO,ts,sesion,Linea,Columna);

				}
				else
				{
					return new ThrowError(TipoThrow.Exception,
							"El retorno de la función debe ser de tipo '" + this.tipoRetorno.ToString() + "'",
							Linea, Columna);
				}
			}
			if (this.tipoRetorno!=null) {
				return new ThrowError(TipoThrow.Exception,
							"La función debe retornar algún valor tipo '" + this.tipoRetorno.ToString() + "'",
							Linea, Columna);
			}
			return null;
		}

		public string GetLlave()
		{
			StringBuilder llave = new StringBuilder();
			llave.Append(Nombre + "(");
			int contador = 0;
			foreach (Parametro par in this.parametros)
			{
				if (par.Tipo.Tipo == TipoDatoDB.INT || par.Tipo.Tipo == TipoDatoDB.DOUBLE) {
					llave.Append("numero");
				}
				else if (par.Tipo.Tipo == TipoDatoDB.OBJETO) {
					llave.Append("objeto");
				} else if (par.Tipo.Tipo == TipoDatoDB.MAP_OBJETO || par.Tipo.Tipo==TipoDatoDB.MAP_PRIMITIVO) {
					llave.Append("map");
				}
				else if (par.Tipo.Tipo == TipoDatoDB.LISTA_OBJETO || par.Tipo.Tipo == TipoDatoDB.LISTA_PRIMITIVO)
				{
					llave.Append("list");
				}
				else if (par.Tipo.Tipo == TipoDatoDB.SET_OBJETO || par.Tipo.Tipo == TipoDatoDB.SET_PRIMITIVO)
				{
					llave.Append("set");
				}
				else {
					llave.Append(par.Tipo.ToString());
				}
				if (contador < parametros.Count - 1)
				{
					llave.Append(",");
				}
				contador++;
			}
			llave.Append(")");
			return llave.ToString();
		}

		internal void LimpiarParametros()
		{
			valoresParametros = null;
		}
	}
}
