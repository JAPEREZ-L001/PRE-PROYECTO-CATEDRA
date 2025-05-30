using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO; // Para operaciones de archivos

namespace PROYECTO_CATEDRA_PREG02T
{
    internal class Program
    {
        #region ESTRUCTURA DE DATOS

        // Estructura que representa una cuenta bancaria
        struct CuentaBancaria
        {
            public string NumeroCuenta;
            public string NombreTitular;
            public decimal Saldo;
        }

        #endregion

        #region MÉTODO PRINCIPAL

        static void Main(string[] args)
        {
            CuentaBancaria cuenta = CargarCuenta(); // Se carga la cuenta desde archivo
            string Pin = "0000"; // PIN estático (se puede mejorar almacenando con hash)

            Console.Title = "Proyecto de cátedra: Simulador de un cajero automático";
            Console.WriteLine("\n\tBIENVENIDOS AL CAJERO AUTOMÁTICO BANCO SALESIANO");

            // Hasta 3 intentos de autenticación
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("\nIntento: [" + (i + 1) + "]");
                Console.Write("\nIngrese número de cuenta: ");
                string user = Console.ReadLine();

                Console.Write("Ingrese PIN (cuatro dígitos): ");
                string pass = LeerPinOculto();

                // Verifica si las credenciales coinciden
                if (user == cuenta.NumeroCuenta && pass == Pin)
                {
                    string opcion;
                    do
                    {
                        Console.Clear();
                        Console.WriteLine($"\nBIENVENID@ {cuenta.NombreTitular}");
                        Console.WriteLine("1. Consultar saldo");
                        Console.WriteLine("2. Depositar dinero");
                        Console.WriteLine("3. Retirar dinero");
                        Console.WriteLine("4. Salir del sistema");
                        Console.Write("Seleccione una opción: ");
                        opcion = Console.ReadLine();

                        switch (opcion)
                        {
                            case "1":
                                ConsultarSaldo(cuenta);
                                break;
                            case "2":
                                DepositarDinero(ref cuenta);
                                GuardarCuenta(cuenta);
                                break;
                            case "3":
                                RetirarDinero(ref cuenta);
                                GuardarCuenta(cuenta);
                                break;
                            case "4":
                                Console.WriteLine("Saliendo del sistema...");
                                break;
                            default:
                                Console.WriteLine("Opción inválida.");
                                break;
                        }

                        if (opcion != "4")
                        {
                            Console.WriteLine("Presione ENTER para continuar...");
                            Console.ReadLine();
                        }

                    } while (opcion != "4");
                    break;
                }
                else
                {
                    Console.WriteLine("Datos incorrectos. Intente nuevamente.");
                }

                if (i == 2)
                {
                    Console.WriteLine("\nDemasiados intentos fallidos. Saliendo del programa...");
                }
            }
        }

        #endregion

        #region FUNCIONES DE OPERACIONES BANCARIAS

        // Muestra el saldo actual
        static void ConsultarSaldo(CuentaBancaria cuenta)
        {
            Console.WriteLine($"Saldo actual: ${cuenta.Saldo}");
        }

        // Permite depositar dinero validando entrada
        static void DepositarDinero(ref CuentaBancaria cuenta)
        {
            Console.Write("Ingrese cantidad a depositar: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal deposito) && deposito > 0)
            {
                cuenta.Saldo += deposito;
                Console.WriteLine("Depósito exitoso.");
                RegistrarTransaccion(cuenta.NumeroCuenta, $"Depósito de ${deposito}");
            }
            else
            {
                Console.WriteLine("Entrada inválida. Solo se permiten números positivos.");
            }
        }

        // Permite retirar dinero validando fondos
        static void RetirarDinero(ref CuentaBancaria cuenta)
        {
            Console.Write("Ingrese cantidad a retirar: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal retiro) && retiro > 0)
            {
                if (retiro <= cuenta.Saldo)
                {
                    cuenta.Saldo -= retiro;
                    Console.WriteLine("Retiro exitoso.");
                    RegistrarTransaccion(cuenta.NumeroCuenta, $"Retiro de ${retiro}");
                }
                else
                {
                    Console.WriteLine("Fondos insuficientes.");
                }
            }
            else
            {
                Console.WriteLine("Entrada inválida. Solo se permiten números positivos.");
            }
        }

        #endregion

        #region FUNCIONES DE AUTENTICACIÓN Y FORMATO

        // Permite leer un PIN de forma oculta con validación de 4 dígitos
        static string LeerPinOculto()
        {
            string pin = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    if (char.IsDigit(key.KeyChar) && pin.Length < 4)
                    {
                        pin += key.KeyChar;
                        Console.Write("*");
                    }
                }
                else if (key.Key == ConsoleKey.Backspace && pin.Length > 0)
                {
                    pin = pin.Substring(0, pin.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return pin.Length == 4 ? pin : ""; // Devuelve cadena vacía si no son 4 dígitos
        }

        #endregion

        #region FUNCIONES DE ARCHIVO

        // Guarda los datos actuales de la cuenta en un archivo
        static void GuardarCuenta(CuentaBancaria cuenta)
        {
            try
            {
                string datos = $"{cuenta.NumeroCuenta}|{cuenta.NombreTitular}|{cuenta.Saldo}";
                File.WriteAllText("cuenta.txt", datos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al guardar la cuenta: " + ex.Message);
            }
        }

        // Carga la cuenta desde archivo, o crea una nueva si hay errores
        static CuentaBancaria CargarCuenta()
        {
            CuentaBancaria cuenta = new CuentaBancaria();
            try
            {
                if (File.Exists("cuenta.txt"))
                {
                    string datos = File.ReadAllText("cuenta.txt");
                    string[] partes = datos.Split('|');
                    if (partes.Length == 3)
                    {
                        cuenta.NumeroCuenta = partes[0];
                        cuenta.NombreTitular = partes[1];
                        cuenta.Saldo = decimal.Parse(partes[2]);
                    }
                    else
                    {
                        throw new FormatException("Formato de cuenta incorrecto.");
                    }
                }
                else
                {
                    // Si no existe el archivo, se crea una cuenta predeterminada
                    cuenta.NumeroCuenta = "1234";
                    cuenta.NombreTitular = "Juan Pérez";
                    cuenta.Saldo = 1000.00m;
                    GuardarCuenta(cuenta);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar la cuenta. Se creará una cuenta nueva.");
                cuenta.NumeroCuenta = "1234";
                cuenta.NombreTitular = "Juan Pérez";
                cuenta.Saldo = 1000.00m;
                GuardarCuenta(cuenta);
            }
            return cuenta;
        }

        // Registra cada transacción en un archivo de log por cuenta
        static void RegistrarTransaccion(string numeroCuenta, string mensaje)
        {
            string logFile = $"{numeroCuenta}_log.txt";
            string entrada = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}";
            try
            {
                File.AppendAllText(logFile, entrada + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo registrar la transacción: " + ex.Message);
            }
        }

        #endregion
    }
}
