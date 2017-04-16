class AddGpsDataToParkingSpace < ActiveRecord::Migration[5.0]
  def change
  	add_column :parking_spaces, :latitude, :double
  	add_column :parking_spaces, :longitude, :double
  end
end
