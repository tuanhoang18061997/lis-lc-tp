using Microsoft.EntityFrameworkCore;


namespace Management.Models
{
    public partial class LABContext : DbContext
    {
        public LABContext()
        {
        }
        public LABContext(DbContextOptions<LABContext> options) : base(options)
        {
        }

        public virtual DbSet<Hospital> Hospitals { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Doctor> Doctors { get; set; }
        public virtual DbSet<Object> Objects { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Type> Types { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public virtual DbSet<Function> Functions { get; set; }
        public virtual DbSet<UserFunction> UserFunctions { get; set; }
        public virtual DbSet<PrintSample> PrintSamples { get; set; }      
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<TestType> TestTypes { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<Sample> Samples { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Connect> Connects { get; set; }
        public virtual DbSet<Map> Maps { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServiceTest> ServiceTests { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<ResultStandard> ResultStandards { get; set; }
        public virtual DbSet<TestCode> TestCodes { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<WorkOrder> WorkOrders { get; set; }
        public virtual DbSet<ResultXN> ResultXNs { get; set; }
        public virtual DbSet<ResultCDHA> ResultCDHAs { get; set; }
        public virtual DbSet<ImageCDHA> ImageCDHAs { get; set; }
        public virtual DbSet<DigitalSign> DigitalSigns { get; set; }
        public virtual DbSet<ZaloOAConfig> ZaloOAConfigs { get; set; }
        public virtual DbSet<ZaloOATemplate> ZaloOATemplates { get; set; }
        public virtual DbSet<ZnsSendRequest> ZnsSendRequests { get; set; }
        public virtual DbSet<ZnsWebhook> ZnsWebhooks { get; set; }
        public virtual DbSet<ExternalFile> ExternalFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("LABConnectionString"));           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
