class CreateParkingSpaces < ActiveRecord::Migration[5.0]
  def change
    create_table :parking_spaces do |t|
      t.text :parking_coords
      t.text :image_url

      t.timestamps
    end
  end
end
