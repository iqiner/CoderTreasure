



angular.module("phonecat", ["ngRoute", "phonecatFilters", "phonecatService", "PhoneList", "commonLib"])
    .config(["$routeProvider", function ($routeProvider) {
        $routeProvider.when("/Phones", { templateUrl: "Phone-List.html", controller: PhoneListCtrl })
                    .when("/Phone/:id", { templateUrl: "Phone-Detail.html", controller: PhoneDetailCtrl })
                    .otherwise({ redirectTo: "/Phones" });
    }])
    .run(function ($rootScope, Phone, PhoneArray) {
        Phone.query(function (data) {
            PhoneArray.initPhone(data);
        });

        $rootScope.orderProps = ["name","age"];
        $rootScope.query = "";
    });


angular.module("phonecatFilters", [])
    .filter("checkMark", function () {
        return function (input) { return !!input ? "\u2713" : "\u2718"; };
    });

angular.module("phonecatService", ["ngResource"])
    .factory("Phone", function ($resource) {
        return $resource('../Resource/Data.js', null, { query: { method: 'GET' } });
    });


angular.module("PhoneList", [])
        .service("PhoneArray", ["$rootScope", function ($rootScope) {
            var service = {
                phoneList: [{
                    "id": "002",
                    "name": "Nokia X",
                    "img": "Nokia.jpg",
                    "snippet": "Niubility phone!",
                    "age": 2
                }],
                PhoneDetail: null,
                phoneDetails: [],
                initPhone: function (phones) {
                    this.phoneList = phones.phoneList;
                    this.phoneDetails = phones.phoneDetail;
                    this.setPhone(this.phoneDetails[0].id);
                    $rootScope.$broadcast("Phone.Init");
                },
                setPhone: function (id) {
                    this.phoneDetail = $.grep(this.phoneDetails, function (item) { return item.id == id })[0];
                    $rootScope.$broadcast("PhoneDetail.Update");
                }
            };

            return service;
        }]);

angular.module("commonLib", [])
    .service("CommonLib", ["$rootScope", function (scope) {
        var commonObj = {
            "query": "123",
            "orderProp":"name"
        };
        return commonObj;
    }]);

function PhoneListCtrl($scope, PhoneArray, CommonLib) {
    //Phone.query(function (data) {
    //    $scope.phones = data.phoneList;
    //});
    
    $scope.phones = PhoneArray.phoneList;
    $scope.$on("Phone.Init", function (event) {
        $scope.phones = PhoneArray.phoneList;
        $scope.$apply();
    });
    //$scope.query = CommonLib.query;
    //$scope.orderProp = CommonLib.orderProp;
}

function PhoneDetailCtrl($scope, $routeParams, PhoneArray, CommonLib) {
    $scope.id = $routeParams.id;
    PhoneArray.setPhone($scope.id);
    $scope.phone = PhoneArray.phoneDetail;
    
    $scope.mainImage = PhoneArray.phoneDetail.images[0];
    
    //$scope.query = CommonLib.query;
    //$scope.orderProp = CommonLib.orderProp;
    $scope.setDetail = function (detail) {
        $scope.phone = detail;
        $scope.mainImage = detail.images[0];
    }

    $scope.setImage = function (img) {
        $scope.mainImage = img;
    }
    $scope.changePhone = function (id) {
        PhoneArray.setPhone(id);
    }

    $scope.$on("PhoneDetail.Update", function (event) {
        $scope.setDetail(PhoneArray.phoneDetail);
        $scope.$apply();
    });
}