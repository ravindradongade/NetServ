namespace NetServData
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class SqlDataModel : DbContext
    {
        // Your context has been configured to use a 'SqlDataModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'NetServData.SqlDataModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'SqlDataModel' 
        // connection string in the application configuration file.
        public SqlDataModel(string connectionString)
            : base(connectionString)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<MasterDetails> MasterDetail { get; set; }
        public virtual DbSet<NodeDetails> NodeDetails { get; set; }

    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}