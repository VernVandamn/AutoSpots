json.extract! parking_space, :id, :parking_coords, :image_url, :name, :latitude, :longitude
json.url parking_space_url(parking_space, format: :json)