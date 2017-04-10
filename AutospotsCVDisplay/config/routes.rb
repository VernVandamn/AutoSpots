Rails.application.routes.draw do

  get 'about/tutorial'

  get 'about/team'

  resources :parking_space_selectors
  get 'parking_space_selector/selector_tool'

  root "welcome#index"
  # For details on the DSL available within this file, see http://guides.rubyonrails.org/routing.html
end
