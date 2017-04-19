class AddNameToParkingSpaces < ActiveRecord::Migration[5.0]
  def change
  	add_column :parking_spaces, :name, :text
  end
end
