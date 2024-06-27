namespace hoistmt.Models.httpModels
{
    public class RegoData
    {
        public string plate { get; set; }
        public string replacement_plate { get; set; }
        public string year_of_manufacture { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string submodel { get; set; }
        public string vin { get; set; }
        public string chassis { get; set; }
        public string engine_no { get; set; }
        public int cc_rating { get; set; }
        public string main_colour { get; set; }
        public string second_colour { get; set; }
        public string body_style { get; set; }
        public string vehicle_type { get; set; }
        public string reported_stolen { get; set; }
        public string country_of_origin { get; set; }
        public int tare_weight { get; set; }
        public int gross_vehicle_mass { get; set; }
        public long date_of_first_registration_in_nz { get; set; }
        public int no_of_seats { get; set; }
        public int no_of_doors { get; set; }
        public string fuel_type { get; set; }
        public string alternative_fuel_type { get; set; }
        public string cause_of_latest_registration { get; set; }
        public string registered_overseas { get; set; }
        public string previous_country_of_registration { get; set; }
        public string result_of_latest_wof_inspection { get; set; }
        public long expiry_date_of_last_successful_wof { get; set; }
        public string result_of_latest_cof_inspection { get; set; }
        public string expiry_date_of_last_successful_cof { get; set; }
        public string date_of_latest_cof_inspection { get; set; }
        public long date_of_latest_wof_inspection { get; set; }
        public string latest_odometer_reading { get; set; }
        public string licence_type { get; set; }
        public long licence_expiry_date { get; set; }
        public string transmission { get; set; }
        public string number_of_owners_no_traders { get; set; }
        public int power { get; set; }
        public string mvma_model_code { get; set; }
        public Plate[] plates { get; set; }
        public string vehicle_usage { get; set; }
        public int no_of_axles { get; set; }
        public string number_of_owners { get; set; }
        public string drive { get; set; }
        public SafetyEconomy safety_economy { get; set; }
    }

    public class Plate
    {
        public string plate_status { get; set; }
        public string plate_type { get; set; }
        public string registration_plate { get; set; }
        public long effective_date { get; set; }
    }

    public class SafetyEconomy
    {
        public int electric_range { get; set; }
        public int electric_consumption { get; set; }
        public int fuel_stars { get; set; }
        public int fuel_consumption { get; set; }
        public string fuel_promo_badge { get; set; }
        public int co2_stars { get; set; }
        public int co2 { get; set; }
        public int yearly_co2 { get; set; }
        public int driver_safety_stars { get; set; }
        public string driver_safety_test { get; set; }
        public int pollutants_stars { get; set; }
        public string test_regime { get; set; }
        public string safety_promo_badge { get; set; }
        public int ruc_rate { get; set; }
        public int ruc { get; set; }
        public string others_safety_test { get; set; }
        public int others_safety_stars { get; set; }
        public string warning_severity { get; set; }
        public string warning_message { get; set; }
        public string warning_action { get; set; }
        public string warning_details { get; set; }
        public string drive { get; set; }
    }
}
