package pages.app

import org.openqa.selenium.By

import geb.Page

class SubscribePage extends Page {

	static at = { driver.findElement(By.xpath("//*[@id='main-content']/div[1]/div[1]/h3")).getText() == "Subscribe" }
	static url = "/subscribe"
}
