class WelcomeController < ApplicationController
  def index
	  @images = Dir.glob("app/assets/images/spots/*.png")
  end
end
