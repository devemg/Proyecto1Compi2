﻿using com.Analisis;
using com.Analisis.Util;
using Proyecto1Compi2.com.Analisis;
using Proyecto1Compi2.com.AST;
using Proyecto1Compi2.com.db;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto1Compi2
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void ToolStripButton1_Click(object sender, EventArgs e)
		{
			
		}

		private void EjecutarHanoi()
		{
			Console.WriteLine("Hanoi 1,2,3\n************************");
			Hanoi(4,1, 2, 3);
		}

		//Método Torres de Hanoi Recursivo
		void Hanoi(int n, int @origen, int auxiliar, int destino)
		{
			if (n == 1)
				Console.WriteLine("mover disco de " + @origen + " a " + @destino);
			else
			{
				Hanoi(@n - 1, origen, destino, auxiliar);
				Console.WriteLine("mover disco de " + origen + " a " + destino);
				Hanoi(n - 1, auxiliar, origen, destino);
			}
		}

	private void EjecutarAckermann()
		{
			Console.WriteLine("Ackermann con 0,1: " + Ackermann(0, 1));
			Console.WriteLine("Ackermann con 1,0: " + Ackermann(1, 0));
			Console.WriteLine("Ackermann con 1,3: " + Ackermann(1, 3));
			Console.WriteLine("Ackermann con 2,4: " + Ackermann(2, 4));
			Console.WriteLine("Ackermann con 5,5: " + Ackermann(5, 5));
		}

		private int Ackermann(int m, int n)
		{
			Console.WriteLine("m= "+m+" n="+n);
			if (m == 0)
				return (n + 1);
			else if (n == 0)
				return (Ackermann(m - 1, 1));
			else
				return (Ackermann(m - 1, Ackermann(m, n - 1)));
		}

		private void Bt_EjecutarLup_Click_1(object sender, EventArgs e)
		{
			textBox2.Clear();
			GeneradorLup.Analizar("[+QUERY][+USER]admin[-USER][+DATA]"+this.richTextBox1.Text+ "[-DATA][-QUERY]");
			foreach (Error error in Analizador.ErroresCQL) {
				textBox2.AppendText(error.ToString());
			}
			//textBox2.AppendText(GeneradorLup.Resultado.ToString());
		}

		private void Btn_LimpiarDB_Click(object sender, EventArgs e)
		{
			Analizador.Clear();
			//textBox1.Clear();
		}

		private void Btn_GenerarArchivos_Click(object sender, EventArgs e)
		{
			Analizador.GenerarArchivos("ArchivoPrincipal.txt");
		}

		public static void MostrarMensajeAUsuario(string mensaje) {
			//textBox2.AppendText(mensaje);
			//textBox2.AppendText("\r\n");
		}

		private void toolStripButton1_Click_1(object sender, EventArgs e)
		{
			//Console.WriteLine("Ackermann con 0,1: " + Ackermann(0, 1));
			//Console.WriteLine("Ackermann con 1,0: " + Ackermann(1, 0));
			//Console.WriteLine("Ackermann con 1,3: " + Ackermann(1, 3));
			//Console.WriteLine("Ackermann con 2,4: " + Ackermann(2, 4));
			//Console.WriteLine("Ackermann con 5,5: " + Ackermann(5, 5));
		}

		private void toolStripButton1_Click_2(object sender, EventArgs e)
		{
			textBox2.Clear();
			GeneradorLup.Analizar(this.richTextBox1.Text);
			textBox2.AppendText(GeneradorLup.Resultado.ToString());
		}
	}
}
