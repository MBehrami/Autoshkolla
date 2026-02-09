using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Helper;
using AdminApi.Models.Vehicle;
using AdminApi.ViewModels.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<Vehicle> _vehicleRepo;
        private readonly ISqlRepository<VehicleFuel> _fuelRepo;
        private readonly ISqlRepository<VehicleService> _serviceRepo;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(
            AppDbContext context,
            ISqlRepository<Vehicle> vehicleRepo,
            ISqlRepository<VehicleFuel> fuelRepo,
            ISqlRepository<VehicleService> serviceRepo,
            ILogger<VehiclesController> logger)
        {
            _context = context;
            _vehicleRepo = vehicleRepo;
            _fuelRepo = fuelRepo;
            _serviceRepo = serviceRepo;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // VEHICLE LIST
        // ─────────────────────────────────────────────────────────────

        /// <summary>Get vehicles. Admin can filter by status (all/active/inactive) and search text.</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetVehiclesList(string? search, string? status)
        {
            try
            {
                var query = _context.Vehicles.AsQueryable();

                // Status filter: "active" (default), "inactive", "all"
                if (string.IsNullOrWhiteSpace(status) || status.Equals("active", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(v => v.IsActive);
                else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(v => !v.IsActive);
                // "all" → no filter

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(v =>
                        (v.PlateNumber != null && v.PlateNumber.ToLower().Contains(s)) ||
                        (v.Brand != null && v.Brand.ToLower().Contains(s)) ||
                        (v.Type != null && v.Type.ToLower().Contains(s)) ||
                        (v.ChassisNumber != null && v.ChassisNumber.ToLower().Contains(s)));
                }

                var list = await query
                    .OrderByDescending(v => v.DateAdded)
                    .Select(v => new
                    {
                        v.VehicleId,
                        v.PlateNumber,
                        v.ChassisNumber,
                        v.Color,
                        v.Type,
                        v.Brand,
                        v.RegistrationDate,
                        v.ExpiryDate,
                        v.CertificateNumber,
                        v.IsActive,
                        v.DateAdded
                    })
                    .ToListAsync();

                return Ok(new { dataCount = list.Count, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVehiclesList failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>Get vehicle details by id.</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetVehicleDetails(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Vehicle not found" });
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>Create a new vehicle.</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateVehicle(AddVehicleRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PlateNumber))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Plate Number is required" });

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                var vehicle = new Vehicle
                {
                    PlateNumber = request.PlateNumber.Trim(),
                    ChassisNumber = request.ChassisNumber?.Trim(),
                    Color = request.Color?.Trim(),
                    Type = request.Type?.Trim(),
                    Brand = request.Brand?.Trim(),
                    RegistrationDate = request.RegistrationDate?.Trim(),
                    ExpiryDate = request.ExpiryDate?.Trim(),
                    CertificateNumber = request.CertificateNumber?.Trim(),
                    IsActive = true,
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Vehicle added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateVehicle failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Failed to save vehicle. " + ex.Message });
            }
        }

        /// <summary>Update an existing vehicle.</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVehicle(int id, AddVehicleRequest request)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Vehicle not found" });

                if (string.IsNullOrWhiteSpace(request.PlateNumber))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Plate Number is required" });

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                vehicle.PlateNumber = request.PlateNumber.Trim();
                vehicle.ChassisNumber = request.ChassisNumber?.Trim();
                vehicle.Color = request.Color?.Trim();
                vehicle.Type = request.Type?.Trim();
                vehicle.Brand = request.Brand?.Trim();
                vehicle.RegistrationDate = request.RegistrationDate?.Trim();
                vehicle.ExpiryDate = request.ExpiryDate?.Trim();
                vehicle.CertificateNumber = request.CertificateNumber?.Trim();
                vehicle.LastUpdatedBy = currentUserId;
                vehicle.LastUpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Vehicle updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateVehicle failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Failed to update vehicle. " + ex.Message });
            }
        }

        /// <summary>Toggle vehicle active/inactive (soft delete).</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> ToggleVehicleStatus(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Vehicle not found" });

                vehicle.IsActive = !vehicle.IsActive;
                vehicle.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                vehicle.LastUpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var label = vehicle.IsActive ? "activated" : "deactivated";
                return Ok(new Confirmation { Status = "success", ResponseMsg = $"Vehicle {label} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleVehicleStatus failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // VEHICLE FUEL
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get fuel entries. Admin sees ALL; Instructor sees ONLY their own.
        /// Supports filters: search, vehicleId, dateFrom (dd.MM.yyyy), dateTo (dd.MM.yyyy).
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet]
        public async Task<ActionResult> GetVehicleFuelList(string? search, int? vehicleId, string? dateFrom, string? dateTo)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var baseQuery = _context.VehicleFuels.AsQueryable();

                // Instructor: only their own fuel entries
                if (role == "Instructor")
                    baseQuery = baseQuery.Where(f => f.StaffUserId == currentUserId);

                // Filter by vehicle
                if (vehicleId.HasValue && vehicleId.Value > 0)
                    baseQuery = baseQuery.Where(f => f.VehicleId == vehicleId.Value);

                // Filter by date range (FillDate is stored as "dd.MM.yyyy" string)
                // We compare by parsing or by string ordering after normalising
                if (!string.IsNullOrWhiteSpace(dateFrom) || !string.IsNullOrWhiteSpace(dateTo))
                {
                    // Load IDs that match date range from DB, then filter
                    // Since dates are stored as dd.MM.yyyy strings, parse in-memory
                    var allInRange = await baseQuery.Select(f => new { f.VehicleFuelId, f.FillDate }).ToListAsync();
                    var filteredIds = new HashSet<int>();
                    DateTime? fromDt = null, toDt = null;
                    if (!string.IsNullOrWhiteSpace(dateFrom) &&
                        DateTime.TryParseExact(dateFrom.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fd))
                        fromDt = fd;
                    if (!string.IsNullOrWhiteSpace(dateTo) &&
                        DateTime.TryParseExact(dateTo.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var td))
                        toDt = td;

                    foreach (var row in allInRange)
                    {
                        if (DateTime.TryParseExact(row.FillDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            if (fromDt.HasValue && dt < fromDt.Value) continue;
                            if (toDt.HasValue && dt > toDt.Value) continue;
                            filteredIds.Add(row.VehicleFuelId);
                        }
                    }
                    baseQuery = baseQuery.Where(f => filteredIds.Contains(f.VehicleFuelId));
                }

                var query = from f in baseQuery
                            join v in _context.Vehicles on f.VehicleId equals v.VehicleId into vv
                            from v in vv.DefaultIfEmpty()
                            join u in _context.Users on f.StaffUserId equals u.UserId into uu
                            from u in uu.DefaultIfEmpty()
                            select new
                            {
                                f.VehicleFuelId,
                                f.FillDate,
                                f.VehicleId,
                                VehiclePlate = v != null ? v.PlateNumber : null,
                                VehicleBrand = v != null ? v.Brand : null,
                                f.FuelAmount,
                                f.FuelType,
                                f.StaffUserId,
                                StaffName = u != null ? u.FullName : null,
                                f.DateAdded
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(f =>
                        (f.VehiclePlate != null && f.VehiclePlate.ToLower().Contains(s)) ||
                        (f.StaffName != null && f.StaffName.ToLower().Contains(s)) ||
                        (f.FuelType != null && f.FuelType.ToLower().Contains(s)));
                }

                var list = await query.OrderByDescending(f => f.DateAdded).ToListAsync();

                return Ok(new { dataCount = list.Count, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVehicleFuelList failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>
        /// Create a new fuel entry. Instructor: StaffUserId is auto-assigned.
        /// Admin: StaffUserId comes from the request.
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpPost]
        public async Task<ActionResult> CreateVehicleFuel(AddVehicleFuelRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FillDate))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Fill Date is required" });
                if (request.VehicleId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Vehicle is required" });
                if (string.IsNullOrWhiteSpace(request.FuelType))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Fuel Type is required" });

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                int staffUserId;
                if (role == "Instructor")
                {
                    staffUserId = currentUserId;
                }
                else
                {
                    if (request.StaffUserId <= 0)
                        return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Staff is required" });
                    staffUserId = request.StaffUserId;
                }

                var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
                if (vehicle == null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Selected vehicle not found" });

                var entry = new VehicleFuel
                {
                    FillDate = request.FillDate.Trim(),
                    VehicleId = request.VehicleId,
                    FuelAmount = request.FuelAmount,
                    FuelType = request.FuelType.Trim(),
                    StaffUserId = staffUserId,
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                _context.VehicleFuels.Add(entry);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Fuel entry added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateVehicleFuel failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Failed to save fuel entry. " + ex.Message });
            }
        }

        /// <summary>Get vehicles dropdown (id + plate). Only active vehicles shown.</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet]
        public async Task<ActionResult> GetVehiclesDropdown()
        {
            var list = await _context.Vehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.PlateNumber)
                .Select(v => new { v.VehicleId, v.PlateNumber, v.Brand })
                .ToListAsync();
            return Ok(new { data = list });
        }

        /// <summary>Get all users (Admin + Instructor) for Staff dropdown. Admin only.</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetStaffDropdown()
        {
            var list = await (from u in _context.Users
                             join r in _context.UserRole on u.UserRoleId equals r.UserRoleId
                             where u.IsActive == true
                             select new { u.UserId, u.FullName, RoleName = r.RoleName })
                            .OrderBy(u => u.FullName)
                            .ToListAsync();
            return Ok(new { data = list });
        }

        // ─────────────────────────────────────────────────────────────
        // VEHICLE SERVICES (placeholder)
        // ─────────────────────────────────────────────────────────────

        /// <summary>Get all vehicle service entries.</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetVehicleServiceList(string? search)
        {
            try
            {
                var query = from s in _context.VehicleServices
                            join v in _context.Vehicles on s.VehicleId equals v.VehicleId into vv
                            from v in vv.DefaultIfEmpty()
                            select new
                            {
                                s.VehicleServiceId,
                                s.VehicleId,
                                VehiclePlate = v != null ? v.PlateNumber : null,
                                VehicleBrand = v != null ? v.Brand : null,
                                s.ServiceDate,
                                s.Description,
                                s.Cost,
                                s.DateAdded
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s2 = search.Trim().ToLower();
                    query = query.Where(x =>
                        (x.VehiclePlate != null && x.VehiclePlate.ToLower().Contains(s2)) ||
                        (x.Description != null && x.Description.ToLower().Contains(s2)));
                }

                var list = await query.OrderByDescending(x => x.DateAdded).ToListAsync();
                return Ok(new { dataCount = list.Count, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVehicleServiceList failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }
    }
}
