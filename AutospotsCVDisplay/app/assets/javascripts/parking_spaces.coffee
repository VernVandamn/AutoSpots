# Place all the behaviors and hooks related to the matching controller here.
# All this logic will automatically be available in application.js.
# You can use CoffeeScript in this file: http://coffeescript.org/

ready = ->
	$ ->
		$("tr[data-link]").click ->
			window.location.replace($(this).data("link"));

		$("#fots").on "change", ->
			if !$.trim($('.field').val())
				$('#fots').removeClass 'has-error'
			else
				$('#fots').addClass 'has-error'



$(document).on('turbolinks:load', ready)
$(document).on('change', '#parking_space_name', ready)

