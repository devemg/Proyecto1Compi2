﻿using com.Analisis;
using com.Analisis.Util;
using Proyecto1Compi2.com.AST;
using Proyecto1Compi2.com.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Proyecto1Compi2.com.db
{
	class Tabla
	{
		private String nombre;
		List<Columna> columnas;
		private int contadorFilas;
		private List<FilaDatos> data;
		
		public List<Columna> Columnas { get => columnas; set => columnas=value; }
		public int ContadorFilas { get => contadorFilas; }
		public string Nombre { get => nombre; set => nombre = value; }
		internal List<FilaDatos> Data { get => data; set => data = value; }

		public Tabla(String nombre)
		{
			this.Nombre = nombre;
			this.columnas = new List<Columna>();
			this.data = new List<FilaDatos>(); ;
		}

		public Tabla(String nombre, List<Columna> tab)
		{
			this.Nombre = nombre;
			this.columnas = tab;
			this.data = new List<FilaDatos>();
		}

		//*****************************COLUMNAS**************************************************

		public void AgregarColumna(Columna columna)
		{
			columnas.Add(columna);
		}

		public void AgregarColumnaNueva(Columna columna,Sesion sesio, int Linea,int Columna)
		{
			int i;
			for (i = 0; i < contadorFilas; i++)
			{
				object valPre = Declaracion.GetValorPredeterminado(columna.Tipo, sesio, Linea, Columna);
				if (valPre!=null) {
					if (valPre.GetType() == typeof(ThrowError))
					{
						Analizador.ErroresCQL.Add(new Error((ThrowError)valPre));
					}
					else {
						columna.Datos.Add(valPre);
					}
				}
				
			}
			columnas.Add(columna);
		}

		public bool ExisteColumna(string colum)
		{
			foreach (Columna cl in this.columnas)
			{
				if (cl.Nombre.Equals(colum))
				{
					return true;
				}
			}
			return false;
		}

		public void LimpiarColumnas()
		{
			this.columnas.Clear();
		}

		internal void EliminarColumna(string col)
		{
			this.columnas.Remove(BuscarColumna(col));
		}

		internal Columna BuscarColumna(string llave)
		{
			foreach (Columna cl in this.columnas)
			{
				if (cl.Nombre.Equals(llave))
				{
					return cl;
				}
			}
			return null;
		}

		internal int ContarCounters()
		{
			int contador = 0;
			foreach (Columna cl in columnas)
			{
				if (cl.Tipo.Tipo == TipoDatoDB.COUNTER)
				{
					contador++;
				}
			}

			return contador;
		}

		//*****************************OPERACIONES***********************************************

		internal void Truncar()
		{
			contadorFilas = 0;
			foreach (Columna cl in columnas)
			{
				cl.Datos.Clear();
			}
		}

		public void MostrarCabecera()
		{
			Console.WriteLine("_____________________________________________________________");
			Console.WriteLine("|  " + Nombre + "                                               |");
			Console.WriteLine("_____________________________________________________________");
			Console.Write("|");
			foreach (Columna st in this.columnas)
			{
				Console.Write(st.Tipo.ToString().ToLower() + ":" + st.Nombre + "|");
			}
			Console.WriteLine();
			Console.WriteLine("_____________________________________________________________");
		}

		internal void AgregarValores(Queue<object> valores)
		{
			foreach (Columna cl in columnas)
			{
				object valor = valores.Dequeue();
				cl.Datos.Add(valor);
			}
			contadorFilas++;
		}

		internal void EliminarDatos(int i)
		{
			foreach (Columna cl in columnas)
			{
				cl.Datos.RemoveAt(i);
			}
			contadorFilas--;
		}

		internal void MostrarDatos()
		{
			int contador = 0;
			for (contador = 0; contador < contadorFilas; contador++)
			{
				foreach (Columna cl in columnas)
				{
					Console.Write("|" + cl.Datos.ElementAt(contador) + "|");
				}
				Console.WriteLine();
			}
		}

		internal object ValidarPkActualizar(Queue<object> valores,int i, int linea, int columna)
		{
			List<Columna> llaves = GetPks();
			//*****
			StringBuilder llavePrimaria = new StringBuilder();
			int indiceFila;
			for (indiceFila = 0; indiceFila < contadorFilas; indiceFila++)
			{
				bool existe = existeLlaveEnFila(indiceFila, llaves, valores, llavePrimaria);
				if (existe)
				{
					if (indiceFila==i) {
						return true;
					}
					return new ThrowError(TipoThrow.ValuesException,
					"La llave primaria compuesta '" + llavePrimaria.ToString().TrimEnd('+') + "' ya existe",
					linea, columna);
				}
			}
			return true;
		}

		public override string ToString()
		{
			StringBuilder cadena = new StringBuilder();
			cadena.Append("\n<\n");
			cadena.Append("\"CQL-TYPE\" = \"TABLE\",\n");
			cadena.Append("\"NAME\" = \"" + Nombre + "\",\n");
			cadena.Append("\"COLUMNS\" =[");
			//columnas
			IEnumerator<Columna> enumerator = columnas.GetEnumerator();
			bool hasNext = enumerator.MoveNext();
			while (hasNext)
			{
				Columna i = enumerator.Current;
				cadena.Append(i.ToString());
				hasNext = enumerator.MoveNext();
				if (hasNext)
				{
					cadena.Append(",");
				}
			}
			enumerator.Dispose();
			//********
			cadena.Append("],\n");
			cadena.Append("\"DATA\" =[");
			int indice = 0;
			int cont;
			while (indice < ContadorFilas)
			{
				cadena.Append("\n<\n");
				cont = 0;
				foreach (Columna cl in this.columnas)
				{
					if (cl.Tipo.Tipo.Equals(TipoDatoDB.STRING))
					{
						if (cl.Datos.ElementAt(indice).Equals("$%_null_%$")) {
							cadena.Append("\"" + cl.Nombre + "\"=null");
						}
						else {
							cadena.Append("\"" + cl.Nombre + "\"=\"" + cl.Datos.ElementAt(indice) + "\"");
						}
					} else if (cl.Tipo.Tipo.Equals(TipoDatoDB.DATE)||cl.Tipo.Tipo.Equals(TipoDatoDB.TIME)) {
						MyDateTime dt = (MyDateTime)cl.Datos.ElementAt(indice);
						if (dt.IsNull) {
							cadena.Append("\"" + cl.Nombre + "\"=" + cl.Datos.ElementAt(indice));
						}
						else {
							cadena.Append("\"" + cl.Nombre + "\"=\'" + cl.Datos.ElementAt(indice) + "\'");
						}
					}
					else
					{
						cadena.Append("\"" + cl.Nombre + "\"=" + cl.Datos.ElementAt(indice).ToString());
					}

					if (cont < this.columnas.Count - 1)
					{
						cadena.Append(",\n");
					}
					cont++;
				 }
				cadena.Append("\n>");
				if (indice < ContadorFilas - 1)
				{
					cadena.Append(",");
				}
				indice++;
			}
			//*******
			cadena.Append("]\n>");
			return cadena.ToString();
		}

		internal void ReemplazarValores(Queue<object> valores, int i)
		{
			int cont = 0;
			foreach (Columna cl in this.columnas) {
				cl.Datos[i] = valores.ElementAt(cont);
				cont++;
			}
		}

		internal object ValidarPk(Queue<object> valoresAInsertar, int linea, int columna)
		{
			List<Columna> llaves = GetPks();
			if (llaves.Count > 1)
			{
				StringBuilder llavePrimaria = new StringBuilder();
				int indiceFila;
				for (indiceFila=0;indiceFila<contadorFilas;indiceFila++) {
					bool existe = existeLlaveEnFila(indiceFila, llaves, valoresAInsertar, llavePrimaria);
					if (existe)
					{
						return new ThrowError(TipoThrow.ValuesException,
						"La llave primaria compuesta '" + llavePrimaria.ToString().TrimEnd('+') + "' ya existe",
						linea, columna);
					}
				}
				return true;
			}
			else if(llaves.Count==1)
			{
				//una sola llave
				if (llaves.ElementAt(0).Tipo.Tipo == TipoDatoDB.COUNTER)
				{
					return true;
				}
				else
				{
					int valor = this.columnas.IndexOf(llaves.ElementAt(0));
					if (llaves.ElementAt(0).Datos.Contains(valoresAInsertar.ElementAt(valor)))
					{
						return new ThrowError(TipoThrow.ValuesException,
						"El valor '" + valoresAInsertar.ElementAt(valor) + "' no puede repetirse en la columna '"
						+ llaves.ElementAt(0).Nombre + "'",
						linea, columna);
					}
				}
			}
			return true;
		}

		private bool existeLlaveEnFila(int indiceFila, List<Columna> llaves, Queue<object> valoresAInsertar,StringBuilder llavePrimaria)
		{
			//al menos uno coincide
			//comparar todas las filas con los valores
			int sonIguales = 0;
			int indiceValores = 0;
			llavePrimaria.Clear();
			foreach (Columna pk in llaves)
			{
				indiceValores = this.columnas.IndexOf(pk);
				if (pk.Datos.ElementAt(indiceFila).Equals(valoresAInsertar.ElementAt(indiceValores)))
				{
					llavePrimaria.Append("[" + pk.Nombre + "=" + valoresAInsertar.ElementAt(indiceValores) + "]" + "+");
					sonIguales++;
				}
			}
			return sonIguales == llaves.Count;
		}

		private List<Columna> GetPks()
		{
			List<Columna> pks = new List<Columna>();
			foreach (Columna cl in this.columnas)
			{
				if (cl.IsPrimary)
				{
					pks.Add(cl);
				}
			}
			return pks;
		}
	}
}
