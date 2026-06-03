using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Models;

public partial class RailwayContext : DbContext
{
    public RailwayContext()
    {
    }

    public RailwayContext(DbContextOptions<RailwayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarType> CarTypes { get; set; }

    public virtual DbSet<CarTypeAllowedCharacteristic> CarTypeAllowedCharacteristics { get; set; }

    public virtual DbSet<CarTypeName> CarTypeNames { get; set; }

    public virtual DbSet<ModernizationStage> ModernizationStages { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteStation> RouteStations { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatCharacteristic> SeatCharacteristics { get; set; }

    public virtual DbSet<SeatCharacteristicMap> SeatCharacteristicMaps { get; set; }

    public virtual DbSet<SeatCharacteristicType> SeatCharacteristicTypes { get; set; }

    public virtual DbSet<SeatPriority> SeatPriorities { get; set; }

    public virtual DbSet<Segment> Segments { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Train> Trains { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=Railway;Username=postgres;Password=1602");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cars_pkey");

            entity.ToTable("cars");

            entity.HasIndex(e => new { e.TrainId, e.CarNumber }, "cars_train_id_car_number_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CarNumber).HasColumnName("car_number");
            entity.Property(e => e.CarTypeId).HasColumnName("car_type_id");
            entity.Property(e => e.SeatsCount).HasColumnName("seats_count");
            entity.Property(e => e.TrainId).HasColumnName("train_id");

            entity.HasOne(d => d.CarType).WithMany(p => p.Cars)
                .HasForeignKey(d => d.CarTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cars_car_type_id_fkey");

            entity.HasOne(d => d.Train).WithMany(p => p.Cars)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cars_train_id_fkey");
        });

        modelBuilder.Entity<CarType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("car_types_pkey");

            entity.ToTable("car_types");

            entity.HasIndex(e => new { e.CarTypeNameId, e.ModernizationStageId }, "car_types_car_type_name_id_modernization_stage_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CarTypeNameId).HasColumnName("car_type_name_id");
            entity.Property(e => e.ModernizationStageId).HasColumnName("modernization_stage_id");
            entity.Property(e => e.PricePerKm).HasColumnName("price_per_km");
            entity.Property(e => e.ServicePrice).HasColumnName("service_price");

            entity.HasOne(d => d.CarTypeName).WithMany(p => p.CarTypes)
                .HasForeignKey(d => d.CarTypeNameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("car_types_car_type_name_id_fkey");

            entity.HasOne(d => d.ModernizationStage).WithMany(p => p.CarTypes)
                .HasForeignKey(d => d.ModernizationStageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("car_types_modernization_stage_id_fkey");
        });

        modelBuilder.Entity<CarTypeAllowedCharacteristic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("car_type_allowed_characteristics_pkey");

            entity.ToTable("car_type_allowed_characteristics");

            entity.HasIndex(e => new { e.CarTypeId, e.SeatCharacteristicId }, "car_type_allowed_characterist_car_type_id_seat_characterist_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CarTypeId).HasColumnName("car_type_id");
            entity.Property(e => e.SeatCharacteristicId).HasColumnName("seat_characteristic_id");

            entity.HasOne(d => d.CarType).WithMany(p => p.CarTypeAllowedCharacteristics)
                .HasForeignKey(d => d.CarTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("car_type_allowed_characteristics_car_type_id_fkey");

            entity.HasOne(d => d.SeatCharacteristic).WithMany(p => p.CarTypeAllowedCharacteristics)
                .HasForeignKey(d => d.SeatCharacteristicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("car_type_allowed_characteristics_seat_characteristic_id_fkey");
        });

        modelBuilder.Entity<CarTypeName>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("car_type_names_pkey");

            entity.ToTable("car_type_names");

            entity.HasIndex(e => e.Name, "car_type_names_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ModernizationStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("modernization_stages_pkey");

            entity.ToTable("modernization_stages");

            entity.HasIndex(e => e.Name, "modernization_stages_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("routes_pkey");

            entity.ToTable("routes");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RouteStation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("route_stations_pkey");

            entity.ToTable("route_stations");

            entity.HasIndex(e => new { e.RouteId, e.StationId }, "route_stations_route_id_station_id_key").IsUnique();

            entity.HasIndex(e => new { e.RouteId, e.StopOrder }, "route_stations_route_id_stop_order_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ArrivalTime).HasColumnName("arrival_time");
            entity.Property(e => e.DayOffset).HasColumnName("day_offset");
            entity.Property(e => e.DepartureTime).HasColumnName("departure_time");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.StopOrder).HasColumnName("stop_order");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteStations)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("route_stations_route_id_fkey");

            entity.HasOne(d => d.Station).WithMany(p => p.RouteStations)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("route_stations_station_id_fkey");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seats_pkey");

            entity.ToTable("seats");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");

            entity.HasOne(d => d.Car).WithMany(p => p.Seats)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seats_car_id_fkey");
        });

        modelBuilder.Entity<SeatCharacteristic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seat_characteristics_pkey");

            entity.ToTable("seat_characteristics");

            entity.HasIndex(e => new { e.CharacteristicTypeId, e.Value }, "seat_characteristics_characteristic_type_id_value_key").IsUnique();

            entity.HasIndex(e => e.Value, "seat_characteristics_value_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CharacteristicTypeId).HasColumnName("characteristic_type_id");
            entity.Property(e => e.Value)
                .HasMaxLength(30)
                .HasColumnName("value");

            entity.HasOne(d => d.CharacteristicType).WithMany(p => p.SeatCharacteristics)
                .HasForeignKey(d => d.CharacteristicTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seat_characteristics_characteristic_type_id_fkey");
        });

        modelBuilder.Entity<SeatCharacteristicMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seat_characteristic_map_pkey");

            entity.ToTable("seat_characteristic_map");

            entity.HasIndex(e => new { e.SeatId, e.SeatCharacteristicId }, "seat_characteristic_map_seat_id_seat_characteristic_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.SeatCharacteristicId).HasColumnName("seat_characteristic_id");
            entity.Property(e => e.SeatId).HasColumnName("seat_id");

            entity.HasOne(d => d.SeatCharacteristic).WithMany(p => p.SeatCharacteristicMaps)
                .HasForeignKey(d => d.SeatCharacteristicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seat_characteristic_map_seat_characteristic_id_fkey");

            entity.HasOne(d => d.Seat).WithMany(p => p.SeatCharacteristicMaps)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seat_characteristic_map_seat_id_fkey");
        });

        modelBuilder.Entity<SeatCharacteristicType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seat_characteristic_types_pkey");

            entity.ToTable("seat_characteristic_types");

            entity.HasIndex(e => e.Name, "seat_characteristic_types_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SeatPriority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seat_priorities_pkey");

            entity.ToTable("seat_priorities");

            entity.HasIndex(e => e.Name, "seat_priorities_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Segment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("segments_pkey");

            entity.ToTable("segments");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Distance).HasColumnName("distance");
            entity.Property(e => e.FromStationId).HasColumnName("from_station_id");
            entity.Property(e => e.ToStationId).HasColumnName("to_station_id");

            entity.HasOne(d => d.FromStation).WithMany(p => p.SegmentFromStations)
                .HasForeignKey(d => d.FromStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("segments_from_station_id_fkey");

            entity.HasOne(d => d.ToStation).WithMany(p => p.SegmentToStations)
                .HasForeignKey(d => d.ToStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("segments_to_station_id_fkey");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stations_pkey");

            entity.ToTable("stations");

            entity.HasIndex(e => new { e.City, e.Name }, "stations_city_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.City)
                .HasMaxLength(25)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(60)
                .HasColumnName("country");
            entity.Property(e => e.Name)
                .HasMaxLength(25)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tickets_pkey");

            entity.ToTable("tickets");

            entity.HasIndex(e => new { e.TripId, e.SeatId }, "tickets_trip_id_seat_id_unique").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.FromStationId).HasColumnName("from_station_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.SeatId).HasColumnName("seat_id");
            entity.Property(e => e.ToStationId).HasColumnName("to_station_id");
            entity.Property(e => e.TripId).HasColumnName("trip_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.FromStation).WithMany(p => p.TicketFromStations)
                .HasForeignKey(d => d.FromStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_from_station_id_fkey");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_seat_id_fkey");

            entity.HasOne(d => d.ToStation).WithMany(p => p.TicketToStations)
                .HasForeignKey(d => d.ToStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_to_station_id_fkey");

            entity.HasOne(d => d.Trip).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_trip_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_user_id_fkey");
        });

        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trains_pkey");

            entity.ToTable("trains");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(40)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trips_pkey");

            entity.ToTable("trips");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ArrivalDate).HasColumnName("arrival_date");
            entity.Property(e => e.DepartureDate).HasColumnName("departure_date");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.TrainId).HasColumnName("train_id");

            entity.HasOne(d => d.Route).WithMany(p => p.Trips)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trips_route_id_fkey");

            entity.HasOne(d => d.Train).WithMany(p => p.Trips)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trips_train_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Surname)
                .HasMaxLength(30)
                .HasColumnName("surname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
