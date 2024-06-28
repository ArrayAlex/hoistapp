using System.ComponentModel.DataAnnotations;


namespace hoistmt.Models.MasterDbModels;

public class Vehicle
{
    [Key]
    public int Id { get; set; }
    public string? plate { get; set; }
    public string? replacement_plate { get; set; }
    public string? year_of_manufacture { get; set; }
    public string? make { get; set; }
    public string? model { get; set; }
    public string? submodel { get; set; }
    public string? vin { get; set; }
    public string? chassis { get; set; }
    public string? engine_no { get; set; }
    public int? cc_rating { get; set; }
    public string? main_colour { get; set; }
    public string? second_colour { get; set; }
    public string? body_style { get; set; }
    public string? vehicle_type { get; set; }
    public string? reported_stolen { get; set; }
    public string? country_of_origin { get; set; }
    public int? tare_weight { get; set; }
    public int? gross_vehicle_mass { get; set; }
    public long? date_of_first_registration_in_nz { get; set; }
    public int? no_of_seats { get; set; }
    public int? no_of_doors { get; set; }
    public string? fuel_type { get; set; }
    public string? alternative_fuel_type { get; set; }
    public string? cause_of_latest_registration { get; set; }
    public string? registered_overseas { get; set; }
    public string? previous_country_of_registration { get; set; }
    public string? result_of_latest_wof_inspection { get; set; }
    public long? expiry_date_of_last_successful_wof { get; set; }
    public string? result_of_latest_cof_inspection { get; set; }
    public string? expiry_date_of_last_successful_cof { get; set; }
    public long? date_of_latest_cof_inspection { get; set; }
    public long? date_of_latest_wof_inspection { get; set; }
    public string? latest_odometer_reading { get; set; }
    public string? licence_type { get; set; }
    public long? licence_expiry_date { get; set; }
    public string? transmission { get; set; }
    public string? number_of_owners_no_traders { get; set; }
    public int? power { get; set; }
    public string? mvma_model_code { get; set; }
    public string? vehicle_usage { get; set; }
    public int? no_of_axles { get; set; }
    public string? number_of_owners { get; set; }
    public string? drive { get; set; }
}