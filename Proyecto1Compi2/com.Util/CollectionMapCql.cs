﻿using Proyecto1Compi2.com.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto1Compi2.com.Util
{
	class CollectionMapCql : Dictionary<object, object>
	{
		TipoObjetoDB tipoLlave;
		TipoObjetoDB tipoValor;
		bool isNull;

		public CollectionMapCql(TipoObjetoDB tipoLlave, TipoObjetoDB tipoValor):base()
		{
			this.tipoLlave = tipoLlave;
			this.tipoValor = tipoValor;
			this.isNull = false;
		}

		public CollectionMapCql()
		{
			this.isNull = true;
			this.tipoLlave = null;
			this.TipoValor = null;
		} 

		public TipoObjetoDB TipoLlave { get => tipoLlave; set => tipoLlave = value; }
		public TipoObjetoDB TipoValor { get => tipoValor; set => tipoValor = value; }
		public bool IsNull { get => isNull; set => isNull = value; }

		internal object AddItem(object clave, object valorr, int linea, int columna)
		{
			try {
				this.Add(clave, valorr);
				//CONSULTA PARA ORDENAR
				return null;
			} catch (ArgumentException) {
				return new ThrowError(Util.TipoThrow.Exception,
										"La clave '" + clave.ToString() + "' ya existe",
										linea, columna);
			}
			
			//ordenar
		}

		internal object GetItem(object nuevo,int linea,int columna)
		{
			try
			{
				return this[nuevo];
			}
			catch (KeyNotFoundException)
			{
				return new ThrowError(Util.TipoThrow.Exception,
					"La clave '" + nuevo + "' no existe",
					linea, columna);
			}
		}

		internal object SetItem(object clave, object valorr, int linea, int columna)
		{
			foreach (KeyValuePair<object,object> valores in this) {
				if (valores.Key.Equals(clave)) {
					this[valores.Key] = valorr;
					return null;
				}
			}
			return new ThrowError(TipoThrow.Exception,
				"La clave '"+clave+"' no existe", linea, columna);
		}

		internal object EliminarItem(object nuevo, int linea, int columna)
		{
			if (this.ContainsKey(nuevo))
			{
				this.Remove(nuevo);
			}
			else {
				return new ThrowError(TipoThrow.Exception,
								"La clave '" + nuevo.ToString() + "' no existe", linea, columna);
			}
			return null;
		}

		public override string ToString()
		{
			StringBuilder cad = new StringBuilder();
			if (!isNull)
			{
				cad.Append("<");
				int i = 0;
				foreach (KeyValuePair<object, object> ib in this)
				{
					//***************************************************
					if (this.tipoLlave.Tipo.Equals(TipoDatoDB.STRING))
					{
						cad.Append("\"" + ib.Key.ToString() + "\"");
					}
					else if (this.tipoLlave.Tipo.Equals(TipoDatoDB.DATE) || this.tipoLlave.Tipo.Equals(TipoDatoDB.TIME))
					{
						if (ib.Key.ToString().Equals("null"))
						{
							cad.Append("null");
						}
						else
						{
							cad.Append("\'" + ib.Key.ToString() + "\'");
						}
					}
					else
					{
						cad.Append(ib.Key.ToString());
					}
					//***************************************************
					cad.Append("=");
					//***************************************************
					if (this.tipoValor.Tipo.Equals(TipoDatoDB.STRING))
					{
						cad.Append("\"" + ib.Value.ToString() + "\"");
					}
					else if (this.tipoValor.Tipo.Equals(TipoDatoDB.DATE) || this.tipoValor.Tipo.Equals(TipoDatoDB.TIME))
					{
						if (ib.Value.ToString().Equals("null"))
						{
							cad.Append("null");
						}
						else
						{
							cad.Append("\'" + ib.Value.ToString() + "\'");
						}
					}
					else
					{
						cad.Append(ib.Value.ToString());
					}
					//***************************************************
					if (i < this.Count - 1)
					{
						cad.Append(",");
					}
					i++;
				}
				cad.Append(">");
			}
			else {
				cad.Append("null");
			}
			return cad.ToString();
		}

		internal string GetLinealizado()
		{
			StringBuilder cad = new StringBuilder();
			if (!isNull)
			{
				cad.Append("{");
				int i = 0;
				foreach (KeyValuePair<object, object> ib in this)
				{
					cad.Append(ib.Key.ToString() + ":");
					//***
					if (ib.Value.GetType() == typeof(CollectionListCql))
					{
						cad.Append(((CollectionListCql)ib.Value).GetLinealizado());
						cad.Append("<br/>");
					}
					else if (ib.Value.GetType() == typeof(CollectionMapCql))
					{
						cad.Append(((CollectionMapCql)ib.Value).GetLinealizado());
						cad.Append("<br/>");
					}
					else if (ib.Value.GetType() == typeof(Objeto))
					{
						cad.Append(((Objeto)ib.Value).GetLinealizado());
						cad.Append("<br/>");
					}
					else
					{
						cad.Append(ib.Value.ToString());
						cad.Append("<br/>");
					}
					if (i < this.Count - 1)
					{
						cad.Append(",");
						cad.Append("<br/>");
					}
					i++;
				}
				cad.Append("}");
			}
			else {
				cad.Append("null");
			}

			return cad.ToString();
		}
	}
}
