using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Models;

namespace ServiceDesk.Api.Data;

public class ServiceDeskContext : DbContext
{
    public ServiceDeskContext(DbContextOptions<ServiceDeskContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
}

