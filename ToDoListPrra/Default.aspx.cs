using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ToDoListPrra
{
    public partial class Default : System.Web.UI.Page
    {
        List<Tarea> tareas = new List<Tarea>();
        List<Tarea> tareasCompletadas = new List<Tarea>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar tareas desde la base de datos
                CargarTareas();

                // Cargar tareas completadas desde la base de datos
                CargarTareasCompletadas();
            }
        }

        // Evento del botón "Guardar Tarea"
        protected void ButtonGuardar_Click(object sender, EventArgs e)
        {
            string tarea = TextBoxTarea.Text.Trim();
            bool esUrgente = CheckBoxEsUrgente.Checked;
            DateTime fechaCreacion = DateTime.Now; // Fecha y hora actual

            if (!string.IsNullOrEmpty(tarea))
            {
                // Insertar la tarea en la base de datos
                InsertarTareaEnBaseDeDatos(tarea, esUrgente, fechaCreacion);

                // Recargar las tareas desde la base de datos y actualizar el GridView
                CargarTareas();

                // Limpiar el TextBox después de guardar la tarea
                TextBoxTarea.Text = string.Empty;
            }
        }

        // Evento cuando se cambia el estado del CheckBox "Completada"
        // ...

        // Evento cuando se cambia el estado del CheckBox "Completada"
        protected void CheckBoxCompletada_CheckedChanged(object sender, EventArgs e)
        {
            lblMensajeError.Text = string.Empty;

            // Obtener el CheckBox que generó el evento
            CheckBox checkBox = (CheckBox)sender;

            // Obtener la fila del GridView que contiene el CheckBox seleccionado
            GridViewRow row = (GridViewRow)checkBox.NamingContainer;

            // Obtener el identificador (Id) de la tarea seleccionada en la fila del GridView
            int tareaId = Convert.ToInt32(GridViewTareas.DataKeys[row.RowIndex].Value);

            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Consultar la tarea en la base de datos para obtener la versión actualizada
            string query = "SELECT Id, Descripcion, EsUrgente, FechaCreacion, Completada FROM Tareas WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Asignar el parámetro para la consulta SQL
                    command.Parameters.AddWithValue("@Id", tareaId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Crear un objeto Tarea con los datos de la base de datos
                            Tarea tareaCompletada = new Tarea
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : string.Empty,
                                EsUrgente = reader["EsUrgente"] != DBNull.Value ? Convert.ToBoolean(reader["EsUrgente"]) : false,
                                FechaCreacion = reader["FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["FechaCreacion"]) : DateTime.MinValue,
                                Completada = reader["Completada"] != DBNull.Value ? Convert.ToBoolean(reader["Completada"]) : false
                            };

                            // Cerrar el DataReader antes de realizar otra consulta
                            reader.Close();

                            // Actualizar el estado de la tarea en la base de datos
                            string updateQuery = "UPDATE Tareas SET Completada = @Completada WHERE Id = @Id";
                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                // Asignar los parámetros para la consulta de actualización
                                updateCommand.Parameters.AddWithValue("@Completada", checkBox.Checked);
                                updateCommand.Parameters.AddWithValue("@Id", tareaId);
                                updateCommand.ExecuteNonQuery();
                            }

                            if (checkBox.Checked)
                            {
                                // Si la tarea está marcada como completada, moverla a la tabla de tareas completadas y eliminarla de la tabla de tareas pendientes
                                string insertQuery = "INSERT INTO TareasCompletadas (Id, Descripcion, EsUrgente, FechaCreacion) VALUES (@Id, @Descripcion, @EsUrgente, @FechaCreacion)";
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    // Asignar los parámetros para la consulta de inserción
                                    insertCommand.Parameters.AddWithValue("@Id", tareaCompletada.Id);
                                    insertCommand.Parameters.AddWithValue("@Descripcion", tareaCompletada.Descripcion);
                                    insertCommand.Parameters.AddWithValue("@EsUrgente", tareaCompletada.EsUrgente);
                                    insertCommand.Parameters.AddWithValue("@FechaCreacion", tareaCompletada.FechaCreacion);
                                    insertCommand.ExecuteNonQuery();
                                }

                                // Eliminar la tarea completada de la tabla de tareas pendientes
                                string deleteQuery = "DELETE FROM Tareas WHERE Id = @Id";
                                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                                {
                                    // Asignar el parámetro para la consulta de eliminación
                                    deleteCommand.Parameters.AddWithValue("@Id", tareaId);
                                    deleteCommand.ExecuteNonQuery();
                                }
                            }

                            // Recargar los GridViews para mostrar los cambios actualizados
                            CargarTareas();
                        }
                        else
                        {
                            lblMensajeError.Text = "Error: La tarea seleccionada no se encuentra en la base de datos.";
                            System.Diagnostics.Debug.WriteLine("Error: La tarea con Id " + tareaId + " no se encontró en la base de datos.");
                        }
                    }
                }
            }
        }


        // Evento del GridViewTareas para realizar acciones en las filas
        protected void GridViewTareas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "MarcarCompletada")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int tareaId = Convert.ToInt32(GridViewTareas.DataKeys[rowIndex].Value);

                // Buscar la tarea en la lista de tareas pendientes
                Tarea tareaCompletada = tareas.Find(t => t.Id == tareaId);
                if (tareaCompletada != null)
                {
                    // Eliminar la tarea de la lista de tareas pendientes
                    tareas.Remove(tareaCompletada);

                    // Agregar la tarea a la lista de tareas completadas
                    tareasCompletadas.Add(tareaCompletada);

                    // Actualizar el estado de la tarea en la base de datos
                    ActualizarEstadoTareaEnBaseDeDatos(tareaId, true); // Marcar como completada (true)

                    // Recargar el GridView para mostrar los cambios actualizados
                    CargarTareas();
                }
            }
        }
        protected void GridViewTareas_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Obtener el valor de urgente
                bool esUrgente = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "EsUrgente"));

                // Aplicar estilos según la urgencia
                if (esUrgente)
                {
                    e.Row.CssClass = "fila-urgente";
                }
                else
                {
                    e.Row.CssClass = "fila-no-urgente";
                }
            }
        }

        // Cargar las tareas desde la base de datos y actualizar el GridView
        private void CargarTareas()
        {
            List<Tarea> tareasUrgentes = new List<Tarea>();
            List<Tarea> tareasNoUrgentes = new List<Tarea>();

            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Consultar las tareas almacenadas en la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Descripcion, EsUrgente, FechaCreacion, Completada FROM Tareas";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tarea tarea = new Tarea
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : string.Empty,
                                EsUrgente = reader["EsUrgente"] != DBNull.Value ? Convert.ToBoolean(reader["EsUrgente"]) : false,
                                FechaCreacion = reader["FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["FechaCreacion"]) : DateTime.MinValue,
                                Completada = reader["Completada"] != DBNull.Value ? Convert.ToBoolean(reader["Completada"]) : false
                            };

                            // Agregar la tarea a la lista correspondiente (urgente o no urgente)
                            if (tarea.EsUrgente)
                            {
                                tareasUrgentes.Add(tarea);
                            }
                            else
                            {
                                tareasNoUrgentes.Add(tarea);
                            }
                        }
                    }
                }
            }

            // Combinar las listas de tareas (poner primero las urgentes y luego las no urgentes)
            tareas.Clear();
            tareas.AddRange(tareasUrgentes);
            tareas.AddRange(tareasNoUrgentes);

            // Mostrar las tareas en el GridView
            GridViewTareas.DataSource = tareas;
            GridViewTareas.DataBind();

            // Cargar las tareas completadas y actualizar el GridView correspondiente
            CargarTareasCompletadas();
        }

        // Cargar las tareas completadas desde la base de datos y actualizar el GridView
        // Cargar las tareas completadas desde la base de datos y actualizar el GridView
        protected void CargarTareasCompletadas()
        {
            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Consultar las tareas completadas almacenadas en la tabla TareasCompletadas
            List<Tarea> tareasCompletadas = new List<Tarea>(); // Crear una lista para almacenar las tareas completadas
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Descripcion, EsUrgente, FechaCreacion FROM tareasCompletadas"; // Asegúrate de que el nombre de la tabla sea correcto aquí
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tarea tareaCompletada = new Tarea
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Descripcion = reader["Descripcion"].ToString(),
                                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                                EsUrgente = Convert.ToBoolean(reader["EsUrgente"]),
                                Completada = true // Marcar como completada (si está en la tabla TareasCompletadas, siempre será completada)
                            };
                            tareasCompletadas.Add(tareaCompletada);
                        }
                    }
                }
            }

            // Mostrar las tareas completadas en el GridView
            GridViewtareasCompletadas.DataSource = tareasCompletadas;
            GridViewtareasCompletadas.DataBind();
        }


        // Insertar una nueva tarea en la base de datos
        private void InsertarTareaEnBaseDeDatos(string descripcion, bool esUrgente, DateTime fechaCreacion)
        {
            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Insertar la tarea en la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Tareas (Descripcion, EsUrgente, Completada, FechaCreacion) VALUES (@Descripcion, @EsUrgente, 0, @FechaCreacion)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Descripcion", descripcion);
                    command.Parameters.AddWithValue("@EsUrgente", esUrgente);
                    command.Parameters.AddWithValue("@FechaCreacion", fechaCreacion);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Actualizar el estado de una tarea en la base de datos (completada o no completada)
        private void ActualizarEstadoTareaEnBaseDeDatos(int tareaId, bool completada)
        {
            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Actualizar el estado de la tarea en la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Tareas SET Completada = @Completada WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Completada", completada);
                    command.Parameters.AddWithValue("@Id", tareaId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Mover una tarea completada de la lista tareas a la lista tareasCompletadas
        private void MoverTareaCompletada(Tarea tareaCompletada)
        {
            tareas.Remove(tareaCompletada);
            tareaCompletada.Completada = true; // Marcar como completada (true)
            tareasCompletadas.Add(tareaCompletada);
        }

        // Evento para eliminar una tarea
        protected void ButtonEliminar_Command(object sender, CommandEventArgs e)
        {
            int tareaId = Convert.ToInt32(e.CommandArgument);

            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Eliminar la tarea de la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Tareas WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", tareaId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            // Después de eliminar la tarea de la base de datos...
            Tarea tareaEliminada = tareas.Find(t => t.Id == tareaId);
            if (tareaEliminada != null)
            {
                tareas.Remove(tareaEliminada);
            }
            else
            {
                Tarea tareaCompletadaEliminada = tareasCompletadas.Find(t => t.Id == tareaId);
                if (tareaCompletadaEliminada != null)
                {
                    tareasCompletadas.Remove(tareaCompletadaEliminada);
                }
            }

            // Actualizar el GridView para reflejar el cambio
            CargarTareas();
        }
        protected void ButtonEliminarCompletada_Command(object sender, CommandEventArgs e)
        {
            int tareaId = Convert.ToInt32(e.CommandArgument);

            // Obtener la cadena de conexión desde el archivo Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Eliminar la tarea completada de la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM tareasCompletadas WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", tareaId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            // Después de eliminar la tarea de la base de datos...
            Tarea tareaEliminada = tareasCompletadas.Find(t => t.Id == tareaId);
            if (tareaEliminada != null)
            {
                tareasCompletadas.Remove(tareaEliminada);
            }

            // Actualizar el GridView para reflejar el cambio
            CargarTareas();
            CargarTareasCompletadas();
        }
        
            // Tu código existente

            protected string GetUrgenteText(bool esUrgente)
            {
                return esUrgente ? "Si" : "No";
            }

            // El resto de tu código existente
        }


    }

