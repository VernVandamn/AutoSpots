# == Schema Information
#
# Table name: parking_spaces
#
#  id             :integer          not null, primary key
#  parking_coords :text
#  image_url      :text
#  created_at     :datetime         not null
#  updated_at     :datetime         not null
#  latitude       :float
#  longitude      :float
#

class ParkingSpace < ApplicationRecord
	validates :image_url, presence: true
	validates :latitude, presence: true, numericality: {only_float: true}
	validates :longitude, presence: true, numericality: {only_float: true}
end