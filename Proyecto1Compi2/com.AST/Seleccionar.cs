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
	class Seleccionar : Sentencia
	{
		List<Expresion> listaAccesos;
		string tabla;
		Where condicion;
		OrderBy order;
		Limit limit;

		public Seleccionar(List<Expresion> listaAccesos, string tabla, int linea, int columna) : base(linea, columna)
		{
			this.listaAccesos = listaAccesos;
			this.tabla = tabla;
			this.condicion = null;
			this.order = null;
			this.limit = null;
		}

		public string Tabla { get => tabla; set => tabla = value; }
		internal List<Expresion> ListaAccesos { get => listaAccesos; set => listaAccesos = value; }
		internal Where PropiedadWhere { get => condicion; set => condicion = value; }
		internal OrderBy PropiedadOrderBy { get => order; set => order = value; }
		internal Limit PropiedadLimit { get => limit; set => limit = value; }

		public override object Ejecutar(TablaSimbolos tb, Sesion sesion)
		{
			if (this.tabla=="errors") {
				return Consultar(Analizador.Errors,tb,sesion);
			}

			//VALIDANDO TABLA
			if (sesion.DBActual != null)
			{
				BaseDatos db = Analizador.BuscarDB(sesion.DBActual);
				if (db!=null) {
					if (db.ExisteTabla(tabla))
					{
						Tabla miTabla = db.BuscarTabla(tabla);
						//VALIDANDO QUE NO HAYA FUNCION DE AGREGACION
						if (this.listaAccesos!=null) {
							foreach (Expresion exp in this.listaAccesos) {
								if (exp.GetType()==typeof(FuncionAgregacionExp)) {
									return new ThrowError(TipoThrow.Exception,"Función de agregación no permitida en select",
										Linea,Columna);
								}
							}
						}
						return Consultar(miTabla, tb, sesion);
					}
					else
					{
						return new ThrowError(Util.TipoThrow.TableAlreadyExists,
							"La tabla '" + tabla + "' no existe",
							Linea, Columna);
					}
				}
			}
			else
			{
				return new ThrowError(Util.TipoThrow.UseBDException,
					"No se puede ejecutar la sentencia porque no hay una base de datos seleccionada",
					Linea, Columna);
			}
			return null;
		}

		private object Consultar(Tabla miTabla,TablaSimbolos ts,Sesion sesion)
		{

			ResultadoConsulta resultado = new ResultadoConsulta();
			//TITULOS
			if (listaAccesos == null)
			{
				foreach (Columna cl in miTabla.Columnas)
				{
					resultado.Titulos.Add(cl.Nombre);
					resultado.Tipos.Add(cl.Tipo);
				}
			}
			else
			{
				int cc;
				resultado.Titulos = new List<string>();
				for (cc = 0; cc < listaAccesos.Count; cc++)
				{
					if (listaAccesos.ElementAt(cc).GetType() == typeof(Acceso))
					{
						string titulo = ((Acceso)listaAccesos.ElementAt(cc)).getTitulo();
						resultado.Titulos.Add(titulo);
						if (miTabla.ExisteColumna(titulo))
						{
							resultado.Tipos.Add(miTabla.BuscarColumna(titulo).Tipo);
						}
						else {
							resultado.Tipos.Add(new TipoObjetoDB(TipoDatoDB.OBJETO,"%%"));
						}
					}
					else {
						if (listaAccesos.ElementAt(cc).GetType() == typeof(Operacion))
						{
							Operacion op = (Operacion)listaAccesos.ElementAt(cc);
							if (op.TipoOp == TipoOperacion.Identificador)
							{
								resultado.Titulos.Add(op.ValorInterno.ToString());
								if (miTabla.ExisteColumna(op.ValorInterno.ToString()))
								{
									resultado.Tipos.Add(miTabla.BuscarColumna(op.ValorInterno.ToString()).Tipo);
								}
								else
								{
									resultado.Tipos.Add(new TipoObjetoDB(TipoDatoDB.OBJETO, "%%"));
								}
							}
							else
							{
								resultado.Titulos.Add("Resultado " + (cc + 1));
								resultado.Tipos.Add(new TipoObjetoDB(TipoDatoDB.OBJETO, "%%"));
							}
						}
						else {
							resultado.Titulos.Add("Resultado " + (cc + 1));
							resultado.Tipos.Add(new TipoObjetoDB(TipoDatoDB.OBJETO, "%%"));
						}
						
					}
				}
			}
			//DATOS
			int i = 0;
			for (i = 0; i < miTabla.ContadorFilas; i++)
			{
				//AGREGANDO FILA A LA TABLA DE SIMBOLOS
				TablaSimbolos local = new TablaSimbolos(ts);
				foreach (Columna cl in miTabla.Columnas)
				{
					object dato = cl.Datos.ElementAt(i);
					Simbolo s;
					if (Datos.IsTipoCompatibleParaAsignar(cl.Tipo, dato))
					{
						
						if (cl.Tipo.Tipo == TipoDatoDB.COUNTER)
						{
							s = new Simbolo(cl.Nombre, dato, new TipoObjetoDB(TipoDatoDB.INT, "int"), Linea, Columna);

						}
						else
						{
							s = new Simbolo(cl.Nombre, dato, cl.Tipo, Linea, Columna);
						}
					}
					else {
						s = new Simbolo(cl.Nombre, dato,new TipoObjetoDB(TipoDatoDB.NULO,"nulo"), Linea, Columna);
					}

					local.AgregarSimbolo(s);

				}
				//SELECCIONANDO LOS DATOS
				if (listaAccesos != null)
				{
					//HAY COLUMNAS
					FilaDatos fila = new FilaDatos();
					//VALORES
					int indiceColumna;
					for (indiceColumna = 0; indiceColumna < listaAccesos.Count; indiceColumna++)
					{
						object val = listaAccesos.ElementAt(indiceColumna).GetValor(local, sesion);
						if (val != null)
						{
							if (val.GetType() == typeof(ThrowError))
							{
								return val;
							}
							if (val.Equals("$%_null_%$"))
							{

								fila.Datos.Add(new ParDatos("", "null"));
							}
							else
							{
								fila.Datos.Add(new ParDatos("", val));
							}
						}
					}
					//EVALUANDO LA CONDICION WHERE SI ES QUE HAY **************************************
					if (PropiedadWhere != null)
					{
						object condicionWhere = PropiedadWhere.GetValor(local, sesion);
						if (condicionWhere != null)
						{
							if (condicionWhere.GetType() == typeof(ThrowError))
							{
								return condicionWhere;
							}
							if ((bool)condicionWhere)
							{
								resultado.Add(fila);
							}
						}
					}
					else
					{
						resultado.Add(fila);
					}
					//*********************************************************************************
				}
				else
				{
					//COMODIN
					Simbolo val;
					FilaDatos fila = new FilaDatos();
					//llenando nombre de columnas
					foreach (Columna cl in miTabla.Columnas)
					{
						val = local.GetSimbolo(cl.Nombre);
						if (val.Valor.Equals("$%_null_%$"))
						{

							fila.Datos.Add(new ParDatos(cl.Nombre, "null"));
						}
						else {
							fila.Datos.Add(new ParDatos(cl.Nombre, val.Valor));
						}
						
					}
					//EVALUANDO LA CONDICION WHERE SI ES QUE HAY **************************************
					if (PropiedadWhere != null)
					{
						object condicionWhere = PropiedadWhere.GetValor(local, sesion);
						if (condicionWhere != null)
						{
							if (condicionWhere.GetType() == typeof(ThrowError))
							{
								return condicionWhere;
							}
							if ((bool)condicionWhere)
							{
								resultado.Add(fila);
							}
						}
					}
					else
					{
						resultado.Add(fila);
					}
					//*********************************************************************************
				}
			}
			if (order!=null) {
				order.PasarResultado(resultado);
				object posibleError = order.Ejecutar(ts,sesion);
				if (posibleError!=null) {
					if (posibleError.GetType()==typeof(ThrowError)) {
						return posibleError;
					}
					resultado = (ResultadoConsulta)posibleError;
				}
			}

			if (limit!=null) {
				limit.PasarResultado(resultado);
				return limit.Ejecutar(ts,sesion);
			}
			//Form1.MostrarMensajeAUsuario(resultado.ToString());
			return resultado;
		}
	}
}
