using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace login_hash_cj3022234
{
    public partial class Form1 : Form
    {


        string connectionString;
        public Form1()
        {
            InitializeComponent();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            connectionString = "Data Source = sqlexpress; Initial Catalog = logins.jullia; ;User ID=aluno;Password=aluno";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Abrindo a conexão
                    connection.Open();
                    MessageBox.Show("Conexão com o banco de dados bem-sucedida!");


                }
                catch (Exception ex)
                {
                    // Se ocorrer algum erro, mostramos a mensagem de erro
                    MessageBox.Show("Erro ao conectar ao banco de dados: " + ex.Message);
                }
                finally
                {
                    // A conexão será fechada automaticamente pelo 'using'
                }
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuarioLogin.Text.Trim();
            string senha = Crypto.sha256encrypt(txtSenhaLogin.Text.Trim());
            string connectionString = "Data Source = sqlexpress; Initial Catalog = logins.jullia; ;User ID=aluno;Password=aluno";
            string query = "SELECT COUNT(*) FROM acessos WHERE usuario = @usuario AND senha = @senha";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@senha", senha);

                conn.Open();

                int resultado = (int)cmd.ExecuteScalar();

                if (resultado > 0)
                {
                    txtUsuarioLogin.Text = String.Empty;
                    txtSenhaLogin.Text = String.Empty;
                    MessageBox.Show("Login realizado com sucesso!");
                }
                else
                {
                    MessageBox.Show("Usuário/Senha incorretos");
                }
            }

        }

        private bool UsuarioExiste(string _nomeUsuario)
        {
            bool existe = false;

            // Defina a string de conexão com o seu banco de dados SQL Server
            string connectionString = "Data Source = sqlexpress; Initial Catalog = logins.jullia; ;User ID=aluno;Password=aluno";

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                try
                {
                    cn.Open();

                    // Consulta SQL para verificar se o nome de usuário já existe
                    string sql = "SELECT COUNT(*) FROM Acessos WHERE usuario = @usuario";
                    using (SqlCommand cmd = new SqlCommand(sql, cn))
                    {
                        // Adiciona o parâmetro para verificar o nome do usuário
                        cmd.Parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar)).Value = _nomeUsuario;

                        // Executa o comando e obtém o resultado
                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            existe = true; // Usuário já existe
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao verificar o nome de usuário: " + ex.Message);
                }
            }

            return existe;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            AdicionarUsuario(txtUsuario.Text, txtSenha.Text, txtConfirmarSenha.Text, txtEmail.Text);
        }

        private void AdicionarUsuario(string _nomeUsuario, string _senha, string _confirmaSenha, string _email)
        {
            // Regex para validar o email
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(_email);

            // Verifica se o nome de usuário já existe no banco de dados
            if (UsuarioExiste(_nomeUsuario))
            {
                MessageBox.Show("O nome do usuário já existe, tente informar outro nome.");
                return;
            }

            // Confirma a senha
            if (_senha != _confirmaSenha)
            {
                MessageBox.Show("A senha não confere.");
            }
            // A senha tem que ter no mínimo 8 caracteres
            else if (_senha.Length < 8)
            {
                MessageBox.Show("A senha deve conter no mínimo 8 caracteres");
            }
            // Se o email não for válido
            else if (!match.Success)
            {
                MessageBox.Show("Email inválido");
            }
            // Se não informou o usuário
            else if (_nomeUsuario == null)
            {
                MessageBox.Show("Você deve informar um usuário");
            }
            // Se estiver tudo certo, então cria o usuário
            else
            {
                // Criptografa a senha
                string _hashSenha = Crypto.sha256encrypt(_senha);

                AdicionaUsuarioNoBD(_nomeUsuario, _hashSenha, _email);

                // Limpa os campos do formulário
                txtUsuario.Text = String.Empty;
                txtSenha.Text = String.Empty;
                txtConfirmarSenha.Text = String.Empty;
                txtEmail.Text = String.Empty;

                MessageBox.Show("Obrigado por seu registro!");
            }
        }
        private void AdicionaUsuarioNoBD(string _nomeUsuario, string _senha, string _email)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))  // AQUI você passa a connectionString
            {
                try
                {
                    // Abrindo a conexão
                    cn.Open();

                    // Definindo o comando SQL para inserir o usuário
                    string sql = "INSERT INTO Acessos (usuario, senha, email) VALUES (@usuario, @senha, @email)";

                    // Criando o comando SQL com parâmetros
                    using (SqlCommand cmd = new SqlCommand(sql, cn))
                    {
                        // Adicionando os parâmetros
                        cmd.Parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar)).Value = _nomeUsuario;
                        cmd.Parameters.Add(new SqlParameter("@senha", SqlDbType.NVarChar)).Value = _senha;
                        cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar)).Value = _email;

                        // Executando o comando no banco de dados
                        cmd.ExecuteNonQuery();
                    }

                    // Mensagem de sucesso
                    MessageBox.Show("Usuário adicionado com sucesso.");
                }
                catch (SqlException sqlexception)
                {
                    // Tratamento de erro
                    MessageBox.Show(sqlexception.Message, "Erro ao adicionar usuário", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // Tratamento de erro genérico
                    MessageBox.Show(ex.Message, "Erro desconhecido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // A conexão será fechada automaticamente pelo 'using'
                }

            }

        }
    }
}
