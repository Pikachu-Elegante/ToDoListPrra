<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ToDoListPrra.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>To-Do List Prron</title>
    <style>
        body {
            position: center;
            margin: auto;
            justify-content:center
           
        }

        body::before {
            content: "";
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: -1;
            background-image: url('received_764332908779295.png');
            background-size:cover;
            background-repeat: repeat;
            opacity: 0.125;
        }

        /* Cambiar el color de texto en los encabezados h2 */
        h1 {
            color: #007bff;
        }
        h2{
            color: rgb(76, 255, 0);
            font-size: 30px;
        }
        /* Cambiar el color del borde del GridViewTareas */
        #GridViewTareas {
            border: 1px solid #ccc;
            border-radius: 5px;
        }

        /* Cambiar el color de fondo de los botones */
        .btn {
            background-color: #007bff;
            color: #fff;
            border: none;
            padding: 8px 16px;
            border-radius: 4px;
            cursor: pointer;
        }

        /* Estilo para los botones de eliminar */
        .btn-danger {
            background-color: rgb(255, 0, 0);
        }
        .container {
            width: 100%; /* Ajusta el ancho según tus necesidades */
            background-color: rgba(255, 255, 255, 0.1); /* Ajusta el nivel de transparencia */
            text-align: center; /* Centrar el texto dentro del contenedor */
            padding:10px;
            margin: 5px; /* Añadir un margen al contenedor */
        }
         .fila-urgente {
            background-color: red;
            color: white;
        }

        /* Estilos para tareas no urgentes */
        .fila-no-urgente {
            background-color: green;
            color: white;
        }
       
    </style>
</head>
<body>
    <form id="form1" runat="server">
         <div class="container">
            <h1>Tareas Pendientes UnU</h1>
            <asp:TextBox ID="TextBoxTarea" runat="server" placeholder="Ingrese una nueva tarea" Width="218px"></asp:TextBox>
            <asp:CheckBox ID="CheckBoxEsUrgente" runat="server" Text="Urgente" />
            <asp:Button ID="ButtonGuardar" runat="server" Text="Guardar Tarea" OnClick="ButtonGuardar_Click" />
        <br/>
        <br/>
             <div style ="text-align:center;">
      <asp:GridView ID="GridViewTareas" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" OnRowCommand="GridViewTareas_RowCommand" OnRowDataBound="GridViewTareas_RowDataBound" Width="662px">
    <Columns>
        <asp:BoundField DataField="Id" HeaderText="ID" />
        <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
        <asp:TemplateField HeaderText="Urgente">
            <ItemTemplate>
                <asp:Label ID="LblUrgente" runat="server" Text='<%# GetUrgenteText((bool)Eval("EsUrgente")) %>' CssClass="urgente" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="FechaCreacion" HeaderText="Fecha de Creación" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
        <asp:TemplateField HeaderText="Completada">
            <ItemTemplate>
                <asp:CheckBox ID="CheckBoxCompletada" runat="server" Checked='<%# Bind("Completada") %>' OnCheckedChanged="CheckBoxCompletada_CheckedChanged" AutoPostBack="true" CommandName="MarcarCompletada" CommandArgument='<%# Eval("Id") %>' />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Acciones">
            <ItemTemplate>
                <asp:Button ID="ButtonEliminar" runat="server" Text="Eliminar" CommandName="EliminarTarea" CommandArgument='<%# Eval("Id") %>' OnCommand="ButtonEliminar_Command" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
        <br />
        <br />
        <asp:Label ID="lblMensajeError" runat="server" ForeColor="Red"></asp:Label>
        <br /><br />
        <h2>Tareas completadas UwU</h2>
        <asp:GridView ID="GridViewtareasCompletadas" runat="server" AutoGenerateColumns="False" Width="657px">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" />
                <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                <asp:TemplateField HeaderText="Era Urgente?">
            <ItemTemplate>
                <asp:Label ID="LblUrgente" runat="server" Text='<%# GetUrgenteText((bool)Eval("EsUrgente")) %>' CssClass="urgente" />
            </ItemTemplate>
        </asp:TemplateField>
                <asp:BoundField DataField="FechaCreacion" HeaderText="Fecha de Creación" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:TemplateField HeaderText="Acciones">
    <ItemTemplate>
        <asp:Button ID="ButtonEliminarCompletada" runat="server" Text="Eliminar" CommandName="EliminarTareaCompletada" CommandArgument='<%# Eval("Id") %>' OnCommand="ButtonEliminarCompletada_Command" />
    </ItemTemplate>
</asp:TemplateField>

            </Columns>
        </asp:GridView>
             </div>
             </div>
    </form>
</body>
</html>
