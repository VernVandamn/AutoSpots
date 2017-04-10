class ParkingSpaceSelectorsController < ApplicationController
  before_action :set_parking_space_selector, only: [:show, :edit, :update, :destroy]

  # GET /parking_space_selectors
  # GET /parking_space_selectors.json
  def index
    @parking_space_selectors = ParkingSpaceSelector.all
  end

  # GET /parking_space_selectors/1
  # GET /parking_space_selectors/1.json
  def show
  end

  # GET /parking_space_selectors/new
  def new
    @parking_space_selector = ParkingSpaceSelector.new
  end

  # GET /parking_space_selectors/1/edit
  def edit
  end

  # POST /parking_space_selectors
  # POST /parking_space_selectors.json
  def create
    @parking_space_selector = ParkingSpaceSelector.new(parking_space_selector_params)

    respond_to do |format|
      if @parking_space_selector.save
        format.html { redirect_to @parking_space_selector, notice: 'Parking space selector was successfully created.' }
        format.json { render :show, status: :created, location: @parking_space_selector }
      else
        format.html { render :new }
        format.json { render json: @parking_space_selector.errors, status: :unprocessable_entity }
      end
    end
  end

  # PATCH/PUT /parking_space_selectors/1
  # PATCH/PUT /parking_space_selectors/1.json
  def update
    respond_to do |format|
      if @parking_space_selector.update(parking_space_selector_params)
        format.html { redirect_to @parking_space_selector, notice: 'Parking space selector was successfully updated.' }
        format.json { render :show, status: :ok, location: @parking_space_selector }
      else
        format.html { render :edit }
        format.json { render json: @parking_space_selector.errors, status: :unprocessable_entity }
      end
    end
  end

  # DELETE /parking_space_selectors/1
  # DELETE /parking_space_selectors/1.json
  def destroy
    @parking_space_selector.destroy
    respond_to do |format|
      format.html { redirect_to parking_space_selectors_url, notice: 'Parking space selector was successfully destroyed.' }
      format.json { head :no_content }
    end
  end

  private
    # Use callbacks to share common setup or constraints between actions.
    def set_parking_space_selector
      @parking_space_selector = ParkingSpaceSelector.find(params[:id])
    end

    # Never trust parameters from the scary internet, only allow the white list through.
    def parking_space_selector_params
      params.require(:parking_space_selector).permit(:content, :user_id)
    end
end
