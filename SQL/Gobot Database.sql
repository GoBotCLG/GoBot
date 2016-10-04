-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema gobot
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema gobot
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `gobot` DEFAULT CHARACTER SET latin1 ;
USE `gobot` ;

-- -----------------------------------------------------
-- Table `gobot`.`shop`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`shop` (
  `IdItem` INT(11) NOT NULL AUTO_INCREMENT,
  `NomItem` VARCHAR(45) NULL DEFAULT NULL,
  `Prix` INT(11) NULL DEFAULT NULL,
  `Disponible` INT(11) NULL DEFAULT NULL,
  PRIMARY KEY (`IdItem`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;


-- -----------------------------------------------------
-- Table `gobot`.`user`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`user` (
  `Username` VARCHAR(45) NOT NULL,
  `Email` VARCHAR(100) NULL DEFAULT NULL,
  `Image` BLOB NULL DEFAULT NULL,
  `Credit` INT(11) NULL DEFAULT NULL,
  `SteamProfile` VARCHAR(100) NULL DEFAULT NULL,
  `Password` CHAR(128) NULL DEFAULT NULL,
  `Win` INT(11) NULL DEFAULT NULL,
  `Game` INT(11) NULL DEFAULT NULL,
  `TotalCredit` INT(11) NULL DEFAULT NULL,
  `EXP` INT(11) NULL DEFAULT NULL,
  `LVL` INT(11) NULL DEFAULT NULL,
  PRIMARY KEY (`Username`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;


-- -----------------------------------------------------
-- Table `gobot`.`achat`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`achat` (
  `User_Username` VARCHAR(45) NOT NULL,
  `Shop_IdItem` INT(11) NOT NULL,
  PRIMARY KEY (`User_Username`, `Shop_IdItem`),
  INDEX `fk_Achat_User_idx` (`User_Username` ASC),
  INDEX `fk_Achat_Shop1_idx` (`Shop_IdItem` ASC),
  CONSTRAINT `fk_Achat_Shop1`
    FOREIGN KEY (`Shop_IdItem`)
    REFERENCES `gobot`.`shop` (`IdItem`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Achat_User`
    FOREIGN KEY (`User_Username`)
    REFERENCES `gobot`.`user` (`Username`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;


-- -----------------------------------------------------
-- Table `gobot`.`bot`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`bot` (
  `IdBot` INT(11) NOT NULL AUTO_INCREMENT,
  `NomBot` VARCHAR(45) NULL DEFAULT NULL,
  `Config` BLOB NULL DEFAULT NULL,
  `KDA` VARCHAR(45) NULL DEFAULT NULL,
  PRIMARY KEY (`IdBot`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;


-- -----------------------------------------------------
-- Table `gobot`.`team`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`team` (
  `IdTeam` INT(11) NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(45) NULL DEFAULT NULL,
  `Win` INT(11) NULL DEFAULT NULL,
  `Game` INT(11) NULL DEFAULT NULL,
  `Bot_IdBot1` INT(11) NOT NULL,
  `Bot_IdBot2` INT(11) NOT NULL,
  `Bot_IdBot3` INT(11) NOT NULL,
  `Bot_IdBot4` INT(11) NOT NULL,
  `Bot_IdBot5` INT(11) NOT NULL,
  PRIMARY KEY (`IdTeam`),
  INDEX `fk_Team_Bot1_idx` (`Bot_IdBot1` ASC),
  INDEX `fk_Team_Bot2_idx` (`Bot_IdBot2` ASC),
  INDEX `fk_Team_Bot3_idx` (`Bot_IdBot3` ASC),
  INDEX `fk_Team_Bot4_idx` (`Bot_IdBot4` ASC),
  INDEX `fk_Team_Bot5_idx` (`Bot_IdBot5` ASC),
  CONSTRAINT `fk_Team_Bot1`
    FOREIGN KEY (`Bot_IdBot1`)
    REFERENCES `gobot`.`bot` (`IdBot`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Team_Bot2`
    FOREIGN KEY (`Bot_IdBot2`)
    REFERENCES `gobot`.`bot` (`IdBot`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Team_Bot3`
    FOREIGN KEY (`Bot_IdBot3`)
    REFERENCES `gobot`.`bot` (`IdBot`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Team_Bot4`
    FOREIGN KEY (`Bot_IdBot4`)
    REFERENCES `gobot`.`bot` (`IdBot`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Team_Bot5`
    FOREIGN KEY (`Bot_IdBot5`)
    REFERENCES `gobot`.`bot` (`IdBot`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;


-- -----------------------------------------------------
-- Table `gobot`.`matchs`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`matchs` (
  `IdMatch` INT(11) NOT NULL AUTO_INCREMENT,
  `Date` DATETIME NULL DEFAULT NULL,
  `Team1` JSON NULL DEFAULT NULL,
  `Team2` JSON NULL DEFAULT NULL,
  `FlagWin` INT(11) NULL DEFAULT NULL,
  `Team_IdTeam1` INT(11) NOT NULL,
  `Team_IdTeam2` INT(11) NOT NULL,
  PRIMARY KEY (`IdMatch`),
  INDEX `fk_Match_Team1_idx` (`Team_IdTeam1` ASC),
  INDEX `fk_Match_Team2_idx` (`Team_IdTeam2` ASC),
  CONSTRAINT `fk_Match_Team1`
    FOREIGN KEY (`Team_IdTeam1`)
    REFERENCES `gobot`.`team` (`IdTeam`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Match_Team2`
    FOREIGN KEY (`Team_IdTeam2`)
    REFERENCES `gobot`.`team` (`IdTeam`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;


-- -----------------------------------------------------
-- Table `gobot`.`bet`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `gobot`.`bet` (
  `IdBet` INT(11) NOT NULL AUTO_INCREMENT,
  `Mise` INT(11) NULL DEFAULT NULL,
  `Profit` INT(11) NULL DEFAULT NULL,
  `User_Username` VARCHAR(45) NOT NULL,
  `Team_IdTeam` INT(11) NOT NULL,
  `Match_IdMatch` INT(11) NOT NULL,
  PRIMARY KEY (`IdBet`),
  INDEX `fk_Bet_User1_idx` (`User_Username` ASC),
  INDEX `fk_Bet_Team1_idx` (`Team_IdTeam` ASC),
  INDEX `fk_Bet_Match1_idx` (`Match_IdMatch` ASC),
  CONSTRAINT `fk_Bet_Match1`
    FOREIGN KEY (`Match_IdMatch`)
    REFERENCES `gobot`.`matchs` (`IdMatch`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Bet_Team1`
    FOREIGN KEY (`Team_IdTeam`)
    REFERENCES `gobot`.`team` (`IdTeam`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Bet_User1`
    FOREIGN KEY (`User_Username`)
    REFERENCES `gobot`.`user` (`Username`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = latin1;

USE `gobot` ;

-- -----------------------------------------------------
-- procedure AddUser
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `AddUser`(PUsername varchar(45), PEmail varchar(100), PImage Blob, PSteamProfile varchar(100), PPasword char(128))
begin 
insert into user (Username,Email,Image,SteamProfile,Password) values (PUsername, PEmail, PImage,PSteamProfile, PPasword);
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure ChangeEmail
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `ChangeEmail`(PUsername varchar(50), NewEmail varchar(125))
begin 
update user set Email = NewEmail where Username = PUsername;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure ChangeImage
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `ChangeImage`(PUsername varchar(50), Newimage Blob)
begin 
update user set Image = Newimage where Username = PUsername;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure ChangePwd
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `ChangePwd`(PUsername varchar(50), PPwd char(128), NewPPwd char(128))
begin 
update user set Password = NewPPwd where Username = PUserame and Password = PPwd;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure ChangeSteamProfile
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `ChangeSteamProfile`(PUsername varchar(50), NewSteamProfile varchar(300))
begin
update user set SteamProfile = NewSteamProfile where Username = PUsername;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure GetPwd
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `GetPwd`(PUsername varchar(50))
begin
select Password from user where Username = PUsername;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure GetTeamCourant
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `GetTeamCourant`()
begin 
select team1, team2 from matchs where Date = sysdate();
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure GetUser
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `GetUser`(Pusername varchar(50), PPassword char(128))
begin 
select * from user where Username = PUsername and Password = PPassword;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure IsUser
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `IsUser`(PUsername varchar(50))
begin 
select count(*) from user where Username = PUsername;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure Removeuser
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `Removeuser`(PUsername varchar(50))
begin
delete from user where Username = PUsername;
end$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure SetTeamCourant
-- -----------------------------------------------------

DELIMITER $$
USE `gobot`$$
CREATE DEFINER=`Sam`@`%` PROCEDURE `SetTeamCourant`(Pteam1 json,Pteam2 json)
begin
update matchs set Team1 = Pteam1, Team2 = Pteam2 where Date = sysdate();
end$$

DELIMITER ;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
